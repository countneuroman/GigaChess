namespace GigaChess.Api.Models;

public class Piece
{
    public PiecesColors Color { get; set; }
    public PiecesRoles Role { get; set; }
    public string Position { get; set; }
}