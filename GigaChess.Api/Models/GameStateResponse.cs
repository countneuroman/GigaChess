namespace GigaChess.Api.Models;

public class GameStateResponse
{
    public required string GameId { get; set; }
    public required string Fen { get; set; }
    public GameStatus Status { get; set; }
    public PiecesColors ActiveColor { get; set; }
    public MoveRecord? LastMove { get; set; }
    public string?[][] Board { get; set; } = [];
}
