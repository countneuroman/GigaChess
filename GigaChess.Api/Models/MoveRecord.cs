namespace GigaChess.Api.Models;

public class MoveRecord
{
    public required string From { get; set; }
    public required string To { get; set; }
    public string? PieceCaptured { get; set; }
    public PiecesRoles? PromotionPiece { get; set; }
    public string? MoveNotation { get; set; }
    public required string FenAfterMove { get; set; }
}
