using GigaChess.Api.Models;

namespace GigaChess.Api.Engine;

public interface IChessEngine
{
    bool IsMoveLegal(GameState state, string from, string to);
    List<Move> GetLegalMoves(GameState state, PiecesColors color);
    bool IsInCheck(GameState state, PiecesColors color);
    bool IsCheckmate(GameState state, PiecesColors color);
    bool IsStalemate(GameState state, PiecesColors color);
}
