namespace GigaChess.Api.Models;

public class MakeMoveRequest
{
    public required string GameId { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
    public PiecesRoles? PromotionPiece { get; set; }
}
