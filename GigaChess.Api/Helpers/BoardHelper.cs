using GigaChess.Api.Models;

namespace GigaChess.Api.Helpers;

public static class BoardHelper
{
    public static string[,] InitStartingPosition()
    {
        var board = new string[8, 8];

        // Row 0 = rank 8 (black pieces)
        board[0, 0] = "bR"; board[0, 1] = "bN"; board[0, 2] = "bB"; board[0, 3] = "bQ";
        board[0, 4] = "bK"; board[0, 5] = "bB"; board[0, 6] = "bN"; board[0, 7] = "bR";

        // Row 1 = rank 7 (black pawns)
        for (int c = 0; c < 8; c++) board[1, c] = "bP";

        // Rows 2-5 empty (null)

        // Row 6 = rank 2 (white pawns)
        for (int c = 0; c < 8; c++) board[6, c] = "wP";

        // Row 7 = rank 1 (white pieces)
        board[7, 0] = "wR"; board[7, 1] = "wN"; board[7, 2] = "wB"; board[7, 3] = "wQ";
        board[7, 4] = "wK"; board[7, 5] = "wB"; board[7, 6] = "wN"; board[7, 7] = "wR";

        return board;
    }

    public static (int Row, int Col) SquareToIndices(string square)
    {
        if (square.Length != 2)
            throw new ArgumentException($"Invalid square notation: {square}");

        int col = square[0] - 'a';
        int row = 8 - (square[1] - '0');

        if (row < 0 || row > 7 || col < 0 || col > 7)
            throw new ArgumentException($"Square out of bounds: {square}");

        return (row, col);
    }

    public static string IndicesToSquare(int row, int col)
    {
        char file = (char)('a' + col);
        char rank = (char)('0' + (8 - row));
        return $"{file}{rank}";
    }

    public static string ToFen(GameState state)
    {
        var parts = new string[6];

        // 1. Piece placement
        var rows = new List<string>();
        for (int r = 0; r < 8; r++)
        {
            var row = "";
            int empty = 0;
            for (int c = 0; c < 8; c++)
            {
                var piece = state.Board[r, c];
                if (piece == null)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0) { row += empty; empty = 0; }
                    row += PieceToFenChar(piece);
                }
            }
            if (empty > 0) row += empty;
            rows.Add(row);
        }
        parts[0] = string.Join("/", rows);

        // 2. Active color
        parts[1] = state.ActiveColor == PiecesColors.White ? "w" : "b";

        // 3. Castling
        var castling = "";
        if (state.CastlingRights.WhiteKingSide) castling += "K";
        if (state.CastlingRights.WhiteQueenSide) castling += "Q";
        if (state.CastlingRights.BlackKingSide) castling += "k";
        if (state.CastlingRights.BlackQueenSide) castling += "q";
        parts[2] = castling.Length > 0 ? castling : "-";

        // 4. En passant
        parts[3] = state.EnPassantSquare ?? "-";

        // 5. Halfmove clock
        parts[4] = state.HalfmoveClock.ToString();

        // 6. Fullmove number
        parts[5] = state.FullmoveNumber.ToString();

        return string.Join(" ", parts);
    }

    public static string ToPositionFen(GameState state)
    {
        // FEN without halfmove/fullmove counters — for threefold repetition check
        var full = ToFen(state);
        var parts = full.Split(' ');
        return string.Join(" ", parts[0], parts[1], parts[2], parts[3]);
    }

    public static void FromFen(string fen, GameState state)
    {
        var parts = fen.Split(' ');
        if (parts.Length != 6)
            throw new ArgumentException($"Invalid FEN: expected 6 parts, got {parts.Length}");

        // 1. Piece placement
        var ranks = parts[0].Split('/');
        if (ranks.Length != 8)
            throw new ArgumentException("Invalid FEN: expected 8 ranks");

        state.Board = new string[8, 8];
        for (int r = 0; r < 8; r++)
        {
            int c = 0;
            foreach (char ch in ranks[r])
            {
                if (char.IsDigit(ch))
                {
                    c += ch - '0';
                }
                else
                {
                    state.Board[r, c] = FenCharToPiece(ch);
                    c++;
                }
            }
        }

        // 2. Active color
        state.ActiveColor = parts[1] == "w" ? PiecesColors.White : PiecesColors.Black;

        // 3. Castling
        var cr = parts[2];
        state.CastlingRights = new CastlingRights(
            cr.Contains('K'), cr.Contains('Q'),
            cr.Contains('k'), cr.Contains('q'));

        // 4. En passant
        state.EnPassantSquare = parts[3] == "-" ? null : parts[3];

        // 5. Halfmove clock
        state.HalfmoveClock = int.Parse(parts[4]);

        // 6. Fullmove number
        state.FullmoveNumber = int.Parse(parts[5]);
    }

    private static char PieceToFenChar(string piece)
    {
        // piece format: "wP", "bK", etc.
        char role = piece[1] switch
        {
            'P' => 'P', 'N' => 'N', 'B' => 'B',
            'R' => 'R', 'Q' => 'Q', 'K' => 'K',
            _ => throw new ArgumentException($"Unknown piece role: {piece[1]}")
        };
        return piece[0] == 'w' ? role : char.ToLower(role);
    }

    private static string FenCharToPiece(char ch)
    {
        char color = char.IsUpper(ch) ? 'w' : 'b';
        char role = char.ToUpper(ch);
        return $"{color}{role}";
    }

    public static PiecesColors GetPieceColor(string piece) =>
        piece[0] == 'w' ? PiecesColors.White : PiecesColors.Black;

    public static PiecesRoles GetPieceRole(string piece) =>
        piece[1] switch
        {
            'P' => PiecesRoles.Pawn,
            'N' => PiecesRoles.Knight,
            'B' => PiecesRoles.Bishop,
            'R' => PiecesRoles.Rook,
            'Q' => PiecesRoles.Queen,
            'K' => PiecesRoles.King,
            _ => throw new ArgumentException($"Unknown piece: {piece}")
        };

    public static string PieceString(PiecesColors color, PiecesRoles role)
    {
        char c = color == PiecesColors.White ? 'w' : 'b';
        char r = role switch
        {
            PiecesRoles.Pawn => 'P',
            PiecesRoles.Knight => 'N',
            PiecesRoles.Bishop => 'B',
            PiecesRoles.Rook => 'R',
            PiecesRoles.Queen => 'Q',
            PiecesRoles.King => 'K',
            _ => throw new ArgumentException($"Unknown role: {role}")
        };
        return $"{c}{r}";
    }
}
