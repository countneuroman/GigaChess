using GigaChess.Api.Helpers;
using GigaChess.Api.Models;

namespace GigaChess.Api.Engine;

public class ChessEngine : IChessEngine
{
    public bool IsMoveLegal(GameState state, string from, string to)
    {
        var (fromRow, fromCol) = BoardHelper.SquareToIndices(from);
        var (toRow, toCol) = BoardHelper.SquareToIndices(to);

        var piece = state.Board[fromRow, fromCol];
        if (piece == null)
            return false;

        if (!IsPseudoLegal(state, fromRow, fromCol, toRow, toCol))
            return false;

        // Verify king is not left in check after the move
        return !LeavesKingInCheck(state, fromRow, fromCol, toRow, toCol);
    }

    public List<Move> GetLegalMoves(GameState state, PiecesColors color)
    {
        var moves = new List<Move>();

        for (int fromRow = 0; fromRow < 8; fromRow++)
        for (int fromCol = 0; fromCol < 8; fromCol++)
        {
            var piece = state.Board[fromRow, fromCol];
            if (piece == null || BoardHelper.GetPieceColor(piece) != color)
                continue;

            for (int toRow = 0; toRow < 8; toRow++)
            for (int toCol = 0; toCol < 8; toCol++)
            {
                if (fromRow == toRow && fromCol == toCol)
                    continue;

                if (!IsPseudoLegal(state, fromRow, fromCol, toRow, toCol))
                    continue;

                if (LeavesKingInCheck(state, fromRow, fromCol, toRow, toCol))
                    continue;

                moves.Add(new Move
                {
                    From = BoardHelper.IndicesToSquare(fromRow, fromCol),
                    To = BoardHelper.IndicesToSquare(toRow, toCol)
                });
            }
        }

        return moves;
    }

    public bool IsInCheck(GameState state, PiecesColors color)
    {
        var (kingRow, kingCol) = FindKing(state.Board, color);
        var enemyColor = color == PiecesColors.White ? PiecesColors.Black : PiecesColors.White;
        return IsSquareAttacked(state.Board, kingRow, kingCol, enemyColor);
    }

    public bool IsCheckmate(GameState state, PiecesColors color)
    {
        return IsInCheck(state, color) && GetLegalMoves(state, color).Count == 0;
    }

    public bool IsStalemate(GameState state, PiecesColors color)
    {
        return !IsInCheck(state, color) && GetLegalMoves(state, color).Count == 0;
    }

    // --- Private helpers ---

    /// <summary>
    /// Checks if a move follows piece movement rules without verifying
    /// whether the king is left in check.
    /// </summary>
    private bool IsPseudoLegal(GameState state, int fromRow, int fromCol, int toRow, int toCol)
    {
        var piece = state.Board[fromRow, fromCol];
        if (piece == null)
            return false;

        var color = BoardHelper.GetPieceColor(piece);
        var role = BoardHelper.GetPieceRole(piece);

        // Can't capture own piece
        var target = state.Board[toRow, toCol];
        if (target != null && BoardHelper.GetPieceColor(target) == color)
            return false;

        return role switch
        {
            PiecesRoles.Pawn => IsPawnMovePseudoLegal(state, color, fromRow, fromCol, toRow, toCol),
            PiecesRoles.Knight => IsKnightMovePseudoLegal(fromRow, fromCol, toRow, toCol),
            PiecesRoles.Bishop => IsBishopMovePseudoLegal(state.Board, fromRow, fromCol, toRow, toCol),
            PiecesRoles.Rook => IsRookMovePseudoLegal(state.Board, fromRow, fromCol, toRow, toCol),
            PiecesRoles.Queen => IsQueenMovePseudoLegal(state.Board, fromRow, fromCol, toRow, toCol),
            PiecesRoles.King => IsKingMovePseudoLegal(state, color, fromRow, fromCol, toRow, toCol),
            _ => false
        };
    }

    private bool IsPawnMovePseudoLegal(GameState state, PiecesColors color, int fromRow, int fromCol, int toRow, int toCol)
    {
        int direction = color == PiecesColors.White ? -1 : 1;
        int startRow = color == PiecesColors.White ? 6 : 1;
        int dRow = toRow - fromRow;
        int dCol = toCol - fromCol;

        // Forward one square
        if (dCol == 0 && dRow == direction && state.Board[toRow, toCol] == null)
            return true;

        // Forward two squares from starting position
        if (dCol == 0 && dRow == 2 * direction && fromRow == startRow
            && state.Board[fromRow + direction, fromCol] == null
            && state.Board[toRow, toCol] == null)
            return true;

        // Diagonal capture (including en passant)
        if (Math.Abs(dCol) == 1 && dRow == direction)
        {
            // Normal capture
            if (state.Board[toRow, toCol] != null)
                return true;

            // En passant
            var epSquare = state.EnPassantSquare;
            if (epSquare != null)
            {
                var (epRow, epCol) = BoardHelper.SquareToIndices(epSquare);
                if (toRow == epRow && toCol == epCol)
                    return true;
            }
        }

        return false;
    }

    private static bool IsKnightMovePseudoLegal(int fromRow, int fromCol, int toRow, int toCol)
    {
        int dr = Math.Abs(toRow - fromRow);
        int dc = Math.Abs(toCol - fromCol);
        return (dr == 2 && dc == 1) || (dr == 1 && dc == 2);
    }

    private static bool IsBishopMovePseudoLegal(string[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        int dr = Math.Abs(toRow - fromRow);
        int dc = Math.Abs(toCol - fromCol);
        if (dr != dc || dr == 0)
            return false;

        return IsPathClear(board, fromRow, fromCol, toRow, toCol);
    }

    private static bool IsRookMovePseudoLegal(string[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        if (fromRow != toRow && fromCol != toCol)
            return false;
        if (fromRow == toRow && fromCol == toCol)
            return false;

        return IsPathClear(board, fromRow, fromCol, toRow, toCol);
    }

    private static bool IsQueenMovePseudoLegal(string[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        return IsBishopMovePseudoLegal(board, fromRow, fromCol, toRow, toCol)
            || IsRookMovePseudoLegal(board, fromRow, fromCol, toRow, toCol);
    }

    private bool IsKingMovePseudoLegal(GameState state, PiecesColors color, int fromRow, int fromCol, int toRow, int toCol)
    {
        int dr = Math.Abs(toRow - fromRow);
        int dc = Math.Abs(toCol - fromCol);

        // Normal king move (one square in any direction)
        if (dr <= 1 && dc <= 1 && (dr + dc > 0))
            return true;

        // Castling: king moves 2 squares horizontally
        if (dr == 0 && dc == 2)
            return IsCastlingPseudoLegal(state, color, fromRow, fromCol, toRow, toCol);

        return false;
    }

    private bool IsCastlingPseudoLegal(GameState state, PiecesColors color, int fromRow, int fromCol, int toRow, int toCol)
    {
        var cr = state.CastlingRights;
        var enemyColor = color == PiecesColors.White ? PiecesColors.Black : PiecesColors.White;
        int row = color == PiecesColors.White ? 7 : 0;

        // King must be on its starting square
        if (fromRow != row || fromCol != 4)
            return false;

        // King must not be in check
        if (IsSquareAttacked(state.Board, fromRow, fromCol, enemyColor))
            return false;

        bool kingSide = toCol > fromCol;

        if (kingSide)
        {
            bool hasRight = color == PiecesColors.White ? cr.WhiteKingSide : cr.BlackKingSide;
            if (!hasRight) return false;

            // Squares f and g must be empty
            if (state.Board[row, 5] != null || state.Board[row, 6] != null)
                return false;

            // Rook must be on h
            if (state.Board[row, 7] == null || BoardHelper.GetPieceRole(state.Board[row, 7]) != PiecesRoles.Rook)
                return false;

            // King does not pass through or end on attacked square
            if (IsSquareAttacked(state.Board, row, 5, enemyColor) ||
                IsSquareAttacked(state.Board, row, 6, enemyColor))
                return false;
        }
        else
        {
            bool hasRight = color == PiecesColors.White ? cr.WhiteQueenSide : cr.BlackQueenSide;
            if (!hasRight) return false;

            // Squares b, c, d must be empty
            if (state.Board[row, 1] != null || state.Board[row, 2] != null || state.Board[row, 3] != null)
                return false;

            // Rook must be on a
            if (state.Board[row, 0] == null || BoardHelper.GetPieceRole(state.Board[row, 0]) != PiecesRoles.Rook)
                return false;

            // King does not pass through or end on attacked square (c and d)
            if (IsSquareAttacked(state.Board, row, 3, enemyColor) ||
                IsSquareAttacked(state.Board, row, 2, enemyColor))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a square is attacked by any piece of the given color.
    /// </summary>
    private static bool IsSquareAttacked(string[,] board, int row, int col, PiecesColors byColor)
    {
        // Check pawn attacks
        int pawnDir = byColor == PiecesColors.White ? 1 : -1; // direction pawns of byColor attack FROM
        string pawn = byColor == PiecesColors.White ? "wP" : "bP";
        int pawnRow = row + pawnDir;
        if (pawnRow >= 0 && pawnRow <= 7)
        {
            if (col - 1 >= 0 && board[pawnRow, col - 1] == pawn) return true;
            if (col + 1 <= 7 && board[pawnRow, col + 1] == pawn) return true;
        }

        // Check knight attacks
        string knight = byColor == PiecesColors.White ? "wN" : "bN";
        int[][] knightOffsets =
        [
            [- 2, -1], [-2, 1], [-1, -2], [-1, 2],
            [1, -2], [1, 2], [2, -1], [2, 1]
        ];
        foreach (var offset in knightOffsets)
        {
            int nr = row + offset[0], nc = col + offset[1];
            if (nr >= 0 && nr <= 7 && nc >= 0 && nc <= 7 && board[nr, nc] == knight)
                return true;
        }

        // Check king attacks
        string king = byColor == PiecesColors.White ? "wK" : "bK";
        for (int dr = -1; dr <= 1; dr++)
        for (int dc = -1; dc <= 1; dc++)
        {
            if (dr == 0 && dc == 0) continue;
            int kr = row + dr, kc = col + dc;
            if (kr >= 0 && kr <= 7 && kc >= 0 && kc <= 7 && board[kr, kc] == king)
                return true;
        }

        // Check sliding pieces (bishop/queen on diagonals, rook/queen on straights)
        string bishop = byColor == PiecesColors.White ? "wB" : "bB";
        string rook = byColor == PiecesColors.White ? "wR" : "bR";
        string queen = byColor == PiecesColors.White ? "wQ" : "bQ";

        // Diagonal directions (bishop + queen)
        int[][] diagonals = [[-1, -1], [-1, 1], [1, -1], [1, 1]];
        foreach (var dir in diagonals)
        {
            if (SlidingAttack(board, row, col, dir[0], dir[1], bishop, queen))
                return true;
        }

        // Straight directions (rook + queen)
        int[][] straights = [[-1, 0], [1, 0], [0, -1], [0, 1]];
        foreach (var dir in straights)
        {
            if (SlidingAttack(board, row, col, dir[0], dir[1], rook, queen))
                return true;
        }

        return false;
    }

    private static bool SlidingAttack(string[,] board, int row, int col, int dRow, int dCol, string piece1, string piece2)
    {
        int r = row + dRow, c = col + dCol;
        while (r >= 0 && r <= 7 && c >= 0 && c <= 7)
        {
            var p = board[r, c];
            if (p != null)
                return p == piece1 || p == piece2;
            r += dRow;
            c += dCol;
        }
        return false;
    }

    /// <summary>
    /// Check if no piece blocks the path between two squares (exclusive of endpoints).
    /// Works for straight lines and diagonals.
    /// </summary>
    private static bool IsPathClear(string[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        int dRow = Math.Sign(toRow - fromRow);
        int dCol = Math.Sign(toCol - fromCol);

        int r = fromRow + dRow, c = fromCol + dCol;
        while (r != toRow || c != toCol)
        {
            if (board[r, c] != null)
                return false;
            r += dRow;
            c += dCol;
        }
        return true;
    }

    private static (int Row, int Col) FindKing(string[,] board, PiecesColors color)
    {
        string king = color == PiecesColors.White ? "wK" : "bK";
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            if (board[r, c] == king)
                return (r, c);
        }
        throw new InvalidOperationException($"King not found for {color}");
    }

    /// <summary>
    /// Simulates a move on a copy of the board and checks if the moving
    /// side's king is left in check.
    /// </summary>
    private bool LeavesKingInCheck(GameState state, int fromRow, int fromCol, int toRow, int toCol)
    {
        var piece = state.Board[fromRow, fromCol];
        var color = BoardHelper.GetPieceColor(piece);
        var enemyColor = color == PiecesColors.White ? PiecesColors.Black : PiecesColors.White;

        // Copy the board
        var board = (string[,])state.Board.Clone();

        // Handle en passant capture
        var role = BoardHelper.GetPieceRole(piece);
        if (role == PiecesRoles.Pawn && state.EnPassantSquare != null)
        {
            var (epRow, epCol) = BoardHelper.SquareToIndices(state.EnPassantSquare);
            if (toRow == epRow && toCol == epCol)
            {
                int capturedPawnRow = color == PiecesColors.White ? toRow + 1 : toRow - 1;
                board[capturedPawnRow, toCol] = null!;
            }
        }

        // Handle castling — move the rook too
        if (role == PiecesRoles.King && Math.Abs(toCol - fromCol) == 2)
        {
            if (toCol > fromCol) // King-side
            {
                board[fromRow, 5] = board[fromRow, 7];
                board[fromRow, 7] = null!;
            }
            else // Queen-side
            {
                board[fromRow, 3] = board[fromRow, 0];
                board[fromRow, 0] = null!;
            }
        }

        // Execute the move
        board[toRow, toCol] = piece;
        board[fromRow, fromCol] = null!;

        // Find king position on the new board
        var (kingRow, kingCol) = FindKing(board, color);
        return IsSquareAttacked(board, kingRow, kingCol, enemyColor);
    }
}
