using GigaChess.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GigaChess.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChessController : Controller
{
    public readonly ILogger<ChessController> _Logger;
    public ChessController(ILogger<ChessController> logger)
    {
        _Logger = logger;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Move), 200)]
    public async Task<IActionResult> RandomMove()
    {
        var move = new Move() { From = "e2", To = "e4" };

        return Ok(move);
    }
    
    [HttpPost]
    [Route("GetPiecePostition")]
    public async Task<IActionResult> GetPiecePostition(Piece piece)
    {
        _Logger.LogInformation($"Piece color: {piece.Color}, piece role: {piece.Role}, piece position: {piece.Position}");
        return Ok();
    }
}