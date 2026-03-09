using System.Collections.Concurrent;
using GigaChess.Api.Common;
using GigaChess.Api.Engine;
using GigaChess.Api.Helpers;
using GigaChess.Api.Models;

namespace GigaChess.Api.Services;

public class GameService
{
    private readonly ConcurrentDictionary<string, GameState> _games;
    private readonly IChessEngine _engine;

    public GameService(ConcurrentDictionary<string, GameState> games, IChessEngine engine)
    {
        _games = games;
        _engine = engine;
    }

    public GameStateResponse CreateGame()
    {
        var state = new GameState
        {
            GameId = Guid.NewGuid().ToString(),
            Board = BoardHelper.InitStartingPosition()
        };

        _games[state.GameId] = state;
        return ToResponse(state);
    }

    public GameStateResponse? GetGame(string gameId)
    {
        return _games.TryGetValue(gameId, out var state) ? ToResponse(state) : null;
    }

    public Result<List<Move>> GetLegalMoves(GetLegalMovesRequest request)
    {
        if (!_games.TryGetValue(request.GameId, out var state))
            return Result<List<Move>>.NotFound("Game not found.");

        var moves = _engine.GetLegalMoves(state, state.ActiveColor);

        if (request.Square != null)
        {
            try
            {
                BoardHelper.SquareToIndices(request.Square);
            }
            catch (ArgumentException)
            {
                return Result<List<Move>>.Fail("Invalid square notation.");
            }

            moves = moves.Where(m => m.From == request.Square).ToList();
        }

        return Result<List<Move>>.Ok(moves);
    }

    public Result<GameStateResponse> MakeMove(MakeMoveRequest request)
    {
        if (!_games.TryGetValue(request.GameId, out var state))
            return Result<GameStateResponse>.NotFound("Game not found.");

        if (state.Status != GameStatus.InProgress)
            return Result<GameStateResponse>.Fail("Game is already finished.");

        // Validate square notation
        int fromRow, fromCol, toRow, toCol;
        try
        {
            (fromRow, fromCol) = BoardHelper.SquareToIndices(request.From);
            (toRow, toCol) = BoardHelper.SquareToIndices(request.To);
        }
        catch (ArgumentException)
        {
            return Result<GameStateResponse>.Fail("Invalid square notation.");
        }

        var piece = state.Board[fromRow, fromCol];
        if (piece == null)
            return Result<GameStateResponse>.Fail("No piece on the source square.");

        // Turn order
        var pieceColor = BoardHelper.GetPieceColor(piece);
        if (pieceColor != state.ActiveColor)
            return Result<GameStateResponse>.Fail($"It is {state.ActiveColor}'s turn.");

        // Engine legality check
        if (!_engine.IsMoveLegal(state, request.From, request.To))
            return Result<GameStateResponse>.Fail("Illegal move.");

        var pieceRole = BoardHelper.GetPieceRole(piece);
        string? capturedPiece = state.Board[toRow, toCol];
        bool isPawnMove = pieceRole == PiecesRoles.Pawn;
        bool isCapture = capturedPiece != null;

        // --- Special moves ---

        // Castling — detect king moving 2 squares
        bool isCastling = false;
        if (pieceRole == PiecesRoles.King && Math.Abs(toCol - fromCol) == 2)
        {
            isCastling = true;
            ExecuteCastling(state, fromRow, fromCol, toRow, toCol);
        }

        // En passant
        if (isPawnMove && request.To == state.EnPassantSquare)
        {
            int capturedPawnRow = pieceColor == PiecesColors.White ? toRow + 1 : toRow - 1;
            capturedPiece = state.Board[capturedPawnRow, toCol];
            state.Board[capturedPawnRow, toCol] = null!;
            isCapture = true;
        }

        // Update en passant square
        if (isPawnMove && Math.Abs(toRow - fromRow) == 2)
        {
            int epRow = (fromRow + toRow) / 2;
            state.EnPassantSquare = BoardHelper.IndicesToSquare(epRow, fromCol);
        }
        else
        {
            state.EnPassantSquare = null;
        }

        // Execute the move (unless castling already handled it)
        if (!isCastling)
        {
            state.Board[toRow, toCol] = piece;
            state.Board[fromRow, fromCol] = null!;
        }

        // Promotion
        if (isPawnMove && (toRow == 0 || toRow == 7))
        {
            if (request.PromotionPiece == null)
                return Result<GameStateResponse>.Fail("Pawn reached the last rank. PromotionPiece is required.");

            if (request.PromotionPiece == PiecesRoles.King || request.PromotionPiece == PiecesRoles.Pawn)
                return Result<GameStateResponse>.Fail("Cannot promote to King or Pawn.");

            state.Board[toRow, toCol] = BoardHelper.PieceString(pieceColor, request.PromotionPiece.Value);
        }

        // Update castling rights
        UpdateCastlingRights(state, piece, request.From);

        // Update halfmove clock
        state.HalfmoveClock = (isPawnMove || isCapture) ? 0 : state.HalfmoveClock + 1;

        // Switch active color
        if (state.ActiveColor == PiecesColors.Black)
            state.FullmoveNumber++;
        state.ActiveColor = state.ActiveColor == PiecesColors.White
            ? PiecesColors.Black
            : PiecesColors.White;

        // Record move
        var moveRecord = new MoveRecord
        {
            From = request.From,
            To = request.To,
            PieceCaptured = capturedPiece,
            PromotionPiece = request.PromotionPiece,
            FenAfterMove = BoardHelper.ToFen(state)
        };
        state.MoveHistory.Add(moveRecord);

        // Draw conditions
        CheckDrawConditions(state);

        // Check for checkmate/stalemate via engine
        if (_engine.IsCheckmate(state, state.ActiveColor))
        {
            state.Status = state.ActiveColor == PiecesColors.White
                ? GameStatus.BlackWins
                : GameStatus.WhiteWins;
        }
        else if (_engine.IsStalemate(state, state.ActiveColor))
        {
            state.Status = GameStatus.Draw;
        }

        return Result<GameStateResponse>.Ok(ToResponse(state));
    }

    private static void ExecuteCastling(GameState state, int fromRow, int fromCol, int toRow, int toCol)
    {
        state.Board[toRow, toCol] = state.Board[fromRow, fromCol];
        state.Board[fromRow, fromCol] = null!;

        if (toCol > fromCol) // King-side
        {
            state.Board[fromRow, 5] = state.Board[fromRow, 7];
            state.Board[fromRow, 7] = null!;
        }
        else // Queen-side
        {
            state.Board[fromRow, 3] = state.Board[fromRow, 0];
            state.Board[fromRow, 0] = null!;
        }
    }

    private static void UpdateCastlingRights(GameState state, string piece, string from)
    {
        var cr = state.CastlingRights;

        if (piece == "wK")
            cr = cr with { WhiteKingSide = false, WhiteQueenSide = false };
        else if (piece == "bK")
            cr = cr with { BlackKingSide = false, BlackQueenSide = false };

        switch (from)
        {
            case "a1": cr = cr with { WhiteQueenSide = false }; break;
            case "h1": cr = cr with { WhiteKingSide = false }; break;
            case "a8": cr = cr with { BlackQueenSide = false }; break;
            case "h8": cr = cr with { BlackKingSide = false }; break;
        }

        state.CastlingRights = cr;
    }

    private void CheckDrawConditions(GameState state)
    {
        if (state.HalfmoveClock >= 100)
        {
            state.Status = GameStatus.Draw;
            return;
        }

        var currentPositionFen = BoardHelper.ToPositionFen(state);
        int repetitions = state.MoveHistory
            .Select(m =>
            {
                var parts = m.FenAfterMove.Split(' ');
                return string.Join(" ", parts[0], parts[1], parts[2], parts[3]);
            })
            .Count(f => f == currentPositionFen);

        if (repetitions >= 3)
        {
            state.Status = GameStatus.Draw;
            return;
        }

        if (IsInsufficientMaterial(state))
        {
            state.Status = GameStatus.Draw;
        }
    }

    private static bool IsInsufficientMaterial(GameState state)
    {
        var pieces = new List<(PiecesColors Color, PiecesRoles Role, int Row, int Col)>();

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var p = state.Board[r, c];
            if (p != null)
                pieces.Add((BoardHelper.GetPieceColor(p), BoardHelper.GetPieceRole(p), r, c));
        }

        var nonKings = pieces.Where(p => p.Role != PiecesRoles.King).ToList();

        if (nonKings.Count == 0)
            return true;

        if (nonKings.Count == 1 &&
            (nonKings[0].Role == PiecesRoles.Bishop || nonKings[0].Role == PiecesRoles.Knight))
            return true;

        if (nonKings.Count == 2 &&
            nonKings.All(p => p.Role == PiecesRoles.Bishop) &&
            nonKings[0].Color != nonKings[1].Color)
        {
            bool sameSquareColor = (nonKings[0].Row + nonKings[0].Col) % 2
                                == (nonKings[1].Row + nonKings[1].Col) % 2;
            if (sameSquareColor)
                return true;
        }

        return false;
    }

    internal static GameStateResponse ToResponse(GameState state) => new()
    {
        GameId = state.GameId,
        Fen = BoardHelper.ToFen(state),
        Status = state.Status,
        ActiveColor = state.ActiveColor,
        LastMove = state.MoveHistory.Count > 0 ? state.MoveHistory[^1] : null,
        Board = BoardToJagged(state.Board)
    };

    private static string?[][] BoardToJagged(string[,] board)
    {
        var result = new string?[8][];
        for (int r = 0; r < 8; r++)
        {
            result[r] = new string?[8];
            for (int c = 0; c < 8; c++)
                result[r][c] = board[r, c];
        }
        return result;
    }
}
