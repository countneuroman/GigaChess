namespace GigaChess.Api.Models;

public class GameState
{
    public required string GameId { get; set; }
    public string[,] Board { get; set; } = new string[8, 8];
    public PiecesColors ActiveColor { get; set; } = PiecesColors.White;
    public List<MoveRecord> MoveHistory { get; set; } = new();
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public string? EnPassantSquare { get; set; }
    public CastlingRights CastlingRights { get; set; } = CastlingRights.All;
    public int HalfmoveClock { get; set; }
    public int FullmoveNumber { get; set; } = 1;
}
