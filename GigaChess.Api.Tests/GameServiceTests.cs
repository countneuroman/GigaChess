using System.Collections.Concurrent;
using GigaChess.Api.Common;
using GigaChess.Api.Engine;
using GigaChess.Api.Helpers;
using GigaChess.Api.Models;
using GigaChess.Api.Services;

namespace GigaChess.Api.Tests;

public class GameServiceTests
{
    private readonly GameService _service;
    private readonly ConcurrentDictionary<string, GameState> _games = new();

    public GameServiceTests()
    {
        _service = new GameService(_games, new ChessEngine());
    }

    private string CreateGame()
    {
        var response = _service.CreateGame();
        return response.GameId;
    }

    private Result<GameStateResponse> Move(string gameId, string from, string to, PiecesRoles? promotion = null)
    {
        return _service.MakeMove(new MakeMoveRequest
        {
            GameId = gameId,
            From = from,
            To = to,
            PromotionPiece = promotion
        });
    }

    // --- CreateGame ---

    [Fact]
    public void CreateGame_ReturnsStartingFen()
    {
        var response = _service.CreateGame();

        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", response.Fen);
        Assert.Equal(GameStatus.InProgress, response.Status);
        Assert.Equal(PiecesColors.White, response.ActiveColor);
    }

    [Fact]
    public void CreateGame_StoresGameInDictionary()
    {
        var response = _service.CreateGame();

        Assert.True(_games.ContainsKey(response.GameId));
    }

    // --- GetGame ---

    [Fact]
    public void GetGame_ExistingGame_ReturnsResponse()
    {
        var gameId = CreateGame();

        var response = _service.GetGame(gameId);

        Assert.NotNull(response);
        Assert.Equal(gameId, response.GameId);
    }

    [Fact]
    public void GetGame_NonExistentGame_ReturnsNull()
    {
        Assert.Null(_service.GetGame("non-existent"));
    }

    // --- Turn order (2ff.2) ---

    [Fact]
    public void MakeMove_WhiteFirst_SwitchesToBlack()
    {
        var gameId = CreateGame();

        var result = Move(gameId, "e2", "e4");

        Assert.True(result.Success);
        Assert.Equal(PiecesColors.Black, result.Data!.ActiveColor);
    }

    [Fact]
    public void MakeMove_BlackSecond_SwitchesToWhite()
    {
        var gameId = CreateGame();
        Move(gameId, "e2", "e4");

        var result = Move(gameId, "e7", "e5");

        Assert.True(result.Success);
        Assert.Equal(PiecesColors.White, result.Data!.ActiveColor);
    }

    [Fact]
    public void MakeMove_WrongColor_Fails()
    {
        var gameId = CreateGame();

        var result = Move(gameId, "e7", "e5"); // Black piece on White's turn

        Assert.False(result.Success);
        Assert.Contains("White", result.Error);
    }

    [Fact]
    public void MakeMove_FullmoveNumber_IncrementsAfterBlack()
    {
        var gameId = CreateGame();
        Move(gameId, "e2", "e4");
        var result = Move(gameId, "e7", "e5");

        // After Black's first move, fullmove = 2
        Assert.Contains("2", result.Data!.Fen.Split(' ')[5]);
    }

    // --- Validation ---

    [Fact]
    public void MakeMove_NonExistentGame_ReturnsNotFound()
    {
        var result = Move("non-existent", "e2", "e4");

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void MakeMove_EmptySquare_Fails()
    {
        var gameId = CreateGame();

        var result = Move(gameId, "e4", "e5"); // No piece on e4

        Assert.False(result.Success);
        Assert.Contains("No piece", result.Error);
    }

    [Fact]
    public void MakeMove_InvalidSquare_Fails()
    {
        var gameId = CreateGame();

        var result = Move(gameId, "z9", "e4");

        Assert.False(result.Success);
        Assert.Contains("Invalid square", result.Error);
    }

    // --- En passant (2ff.4) ---

    [Fact]
    public void MakeMove_PawnDoubleMove_SetsEnPassantSquare()
    {
        var gameId = CreateGame();

        var result = Move(gameId, "e2", "e4");

        Assert.Contains("e3", result.Data!.Fen); // en passant square in FEN
    }

    [Fact]
    public void MakeMove_EnPassantSquare_ClearedAfterNextMove()
    {
        var gameId = CreateGame();
        Move(gameId, "e2", "e4");

        var result = Move(gameId, "d7", "d6"); // non-double pawn move

        var fenParts = result.Data!.Fen.Split(' ');
        Assert.Equal("-", fenParts[3]); // en passant cleared
    }

    [Fact]
    public void MakeMove_EnPassant_CapturesPawn()
    {
        var gameId = CreateGame();
        // Set up en passant: White pawn on e5, Black plays d7→d5
        Move(gameId, "e2", "e4");
        Move(gameId, "a7", "a6"); // filler
        Move(gameId, "e4", "e5");
        Move(gameId, "d7", "d5"); // Black double-moves, en passant on d6

        var result = Move(gameId, "e5", "d6"); // en passant capture

        Assert.True(result.Success);
        // Black pawn on d5 should be gone
        var state = _games[gameId];
        Assert.Null(state.Board[3, 3]); // d5 (row 3, col 3) is empty
        Assert.Equal("wP", state.Board[2, 3]); // White pawn now on d6 (row 2, col 3)
    }

    // --- Castling (2ff.3) ---

    [Fact]
    public void MakeMove_KingMoves_LosesCastlingRights()
    {
        var gameId = CreateGame();
        Move(gameId, "e2", "e4");
        Move(gameId, "e7", "e5");

        // Move king
        Move(gameId, "e1", "e2");

        var state = _games[gameId];
        Assert.False(state.CastlingRights.WhiteKingSide);
        Assert.False(state.CastlingRights.WhiteQueenSide);
        // Black should still have rights
        Assert.True(state.CastlingRights.BlackKingSide);
        Assert.True(state.CastlingRights.BlackQueenSide);
    }

    [Fact]
    public void MakeMove_RookA1Moves_LosesQueenSideCastling()
    {
        var gameId = CreateGame();
        Move(gameId, "a2", "a4");
        Move(gameId, "a7", "a5");

        Move(gameId, "a1", "a3");

        var state = _games[gameId];
        Assert.True(state.CastlingRights.WhiteKingSide);
        Assert.False(state.CastlingRights.WhiteQueenSide);
    }

    [Fact]
    public void MakeMove_KingSideCastle_MovesRook()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        // Clear path for kingside castle
        state.Board[7, 5] = null!; // f1
        state.Board[7, 6] = null!; // g1

        Move(gameId, "e1", "g1"); // castle kingside

        Assert.Equal("wK", state.Board[7, 6]); // King on g1
        Assert.Equal("wR", state.Board[7, 5]); // Rook on f1
        Assert.Null(state.Board[7, 4]); // e1 empty
        Assert.Null(state.Board[7, 7]); // h1 empty
    }

    [Fact]
    public void MakeMove_QueenSideCastle_MovesRook()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        // Clear path for queenside castle
        state.Board[7, 1] = null!; // b1
        state.Board[7, 2] = null!; // c1
        state.Board[7, 3] = null!; // d1

        Move(gameId, "e1", "c1"); // castle queenside

        Assert.Equal("wK", state.Board[7, 2]); // King on c1
        Assert.Equal("wR", state.Board[7, 3]); // Rook on d1
        Assert.Null(state.Board[7, 4]); // e1 empty
        Assert.Null(state.Board[7, 0]); // a1 empty
    }

    // --- Promotion (2ff.5) ---

    [Fact]
    public void MakeMove_PawnReachesLastRank_WithoutPromotion_Fails()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        // Place white pawn on 7th rank
        state.Board[1, 0] = "wP"; // a7
        state.Board[0, 0] = null!; // clear a8

        var result = Move(gameId, "a7", "a8");

        Assert.False(result.Success);
        Assert.Contains("PromotionPiece is required", result.Error);
    }

    [Fact]
    public void MakeMove_PawnPromotion_ToQueen_Works()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.Board[1, 0] = "wP";
        state.Board[0, 0] = null!;

        var result = Move(gameId, "a7", "a8", PiecesRoles.Queen);

        Assert.True(result.Success);
        Assert.Equal("wQ", state.Board[0, 0]);
    }

    [Fact]
    public void MakeMove_PawnPromotion_ToKnight_Works()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.Board[1, 0] = "wP";
        state.Board[0, 0] = null!;

        var result = Move(gameId, "a7", "a8", PiecesRoles.Knight);

        Assert.True(result.Success);
        Assert.Equal("wN", state.Board[0, 0]);
    }

    [Fact]
    public void MakeMove_PawnPromotion_ToKing_Fails()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.Board[1, 0] = "wP";
        state.Board[0, 0] = null!;

        var result = Move(gameId, "a7", "a8", PiecesRoles.King);

        Assert.False(result.Success);
        Assert.Contains("Cannot promote", result.Error);
    }

    [Fact]
    public void MakeMove_PawnPromotion_ToPawn_Fails()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.Board[1, 0] = "wP";
        state.Board[0, 0] = null!;

        var result = Move(gameId, "a7", "a8", PiecesRoles.Pawn);

        Assert.False(result.Success);
        Assert.Contains("Cannot promote", result.Error);
    }

    // --- Draw conditions (2ff.6) ---

    [Fact]
    public void MakeMove_HalfmoveClock_ResetsOnPawnMove()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.HalfmoveClock = 10;

        Move(gameId, "e2", "e4"); // pawn move

        Assert.Equal(0, state.HalfmoveClock);
    }

    [Fact]
    public void MakeMove_HalfmoveClock_ResetsOnCapture()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.HalfmoveClock = 10;
        // Place black piece on d3 for white pawn on e2 to capture diagonally
        state.Board[5, 3] = "bP"; // d3

        Move(gameId, "e2", "d3"); // pawn captures diagonally

        Assert.Equal(0, state.HalfmoveClock);
    }

    [Fact]
    public void MakeMove_HalfmoveClock_IncrementsOnNonPawnNonCapture()
    {
        var gameId = CreateGame();

        // Move knight (non-pawn, non-capture)
        Move(gameId, "b1", "c3");

        var state = _games[gameId];
        Assert.Equal(1, state.HalfmoveClock);
    }

    [Fact]
    public void MakeMove_FiftyMoveRule_DrawAt100Halfmoves()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.HalfmoveClock = 99;

        // Non-pawn, non-capture move → halfmove becomes 100
        Move(gameId, "b1", "c3");

        Assert.Equal(GameStatus.Draw, state.Status);
    }

    [Fact]
    public void MakeMove_InsufficientMaterial_KingVsKing()
    {
        var gameId = CreateGame();
        var state = _games[gameId];

        // Clear board, leave only kings and one piece to capture
        state.Board = new string[8, 8];
        state.Board[7, 4] = "wK";
        state.Board[0, 4] = "bK";
        state.Board[6, 5] = "bP"; // pawn on f2 for king to capture diagonally

        Move(gameId, "e1", "f2"); // king captures pawn → K vs K

        Assert.Equal(GameStatus.Draw, state.Status);
    }

    [Fact]
    public void MakeMove_InsufficientMaterial_KingBishopVsKing()
    {
        var gameId = CreateGame();
        var state = _games[gameId];

        state.Board = new string[8, 8];
        state.Board[7, 4] = "wK";
        state.Board[0, 4] = "bK";
        state.Board[7, 2] = "wB";
        state.Board[6, 5] = "bP"; // pawn on f2 for king to capture

        Move(gameId, "e1", "f2"); // capture → K+B vs K

        Assert.Equal(GameStatus.Draw, state.Status);
    }

    // --- GetLegalMoves ---

    [Fact]
    public void GetLegalMoves_NewGame_Returns20Moves()
    {
        var gameId = CreateGame();

        var result = _service.GetLegalMoves(new GetLegalMovesRequest { GameId = gameId });

        Assert.True(result.Success);
        Assert.Equal(20, result.Data!.Count);
    }

    [Fact]
    public void GetLegalMoves_WithSquare_FiltersToThatPiece()
    {
        var gameId = CreateGame();

        var result = _service.GetLegalMoves(new GetLegalMovesRequest { GameId = gameId, Square = "e2" });

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count); // e3 and e4
        Assert.All(result.Data, m => Assert.Equal("e2", m.From));
    }

    [Fact]
    public void GetLegalMoves_NonExistentGame_ReturnsNotFound()
    {
        var result = _service.GetLegalMoves(new GetLegalMovesRequest { GameId = "missing" });

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void GetLegalMoves_InvalidSquare_Fails()
    {
        var gameId = CreateGame();

        var result = _service.GetLegalMoves(new GetLegalMovesRequest { GameId = gameId, Square = "z9" });

        Assert.False(result.Success);
        Assert.Contains("Invalid square", result.Error);
    }

    // --- Finished game ---

    [Fact]
    public void MakeMove_FinishedGame_Fails()
    {
        var gameId = CreateGame();
        var state = _games[gameId];
        state.Status = GameStatus.Draw;

        var result = Move(gameId, "e2", "e4");

        Assert.False(result.Success);
        Assert.Contains("already finished", result.Error);
    }
}
