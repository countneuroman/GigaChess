namespace GigaChess.Api.Models;

public record CastlingRights(
    bool WhiteKingSide,
    bool WhiteQueenSide,
    bool BlackKingSide,
    bool BlackQueenSide)
{
    public static CastlingRights All => new(true, true, true, true);
    public static CastlingRights None => new(false, false, false, false);
}
