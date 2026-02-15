using GigaChess.Api.Helpers;
using GigaChess.Api.Models;

namespace GigaChess.Api.Engine;

public class StubChessEngine : IChessEngine
{
    public bool IsMoveLegal(GameState state, string from, string to)
    {
        var (fromRow, fromCol) = BoardHelper.SquareToIndices(from);
        var (toRow, toCol) = BoardHelper.SquareToIndices(to);

        // Stub: legal if there's a piece on 'from' and 'to' is within bounds
        var piece = state.Board[fromRow, fromCol];
        return piece != null
            && toRow >= 0 && toRow <= 7
            && toCol >= 0 && toCol <= 7;
    }

    public List<Move> GetLegalMoves(GameState state, PiecesColors color) => new();

    public bool IsInCheck(GameState state, PiecesColors color) => false;

    public bool IsCheckmate(GameState state, PiecesColors color) => false;

    public bool IsStalemate(GameState state, PiecesColors color) => false;
}
