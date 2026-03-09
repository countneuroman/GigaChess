namespace GigaChess.Api.Models;

public class GetLegalMovesRequest
{
    public required string GameId { get; set; }
    public string? Square { get; set; }
}
