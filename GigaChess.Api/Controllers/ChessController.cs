using GigaChess.Api.Models;
using GigaChess.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GigaChess.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChessController : Controller
{
    private readonly ILogger<ChessController> _logger;
    private readonly ChessService _chess;
    public ChessController(ILogger<ChessController> logger, ChessService chess)
    {
        _logger = logger;
        _chess = chess;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Move), 200)]
    public async Task<IActionResult> RandomMove()
    {
        var move = new Move() { From = "e2", To = "e4" };

        return Ok(move);
    }
    
    [HttpPost]
    [Route("GetLegalMovies")]
    public async Task<IActionResult> GetLegalMovies(Piece piece)
    {
        _logger.LogInformation($"Piece color: {piece.Color}, piece role: {piece.Role}, piece position: {piece.Position}");
        var legalMoves = await _chess.GetLogalMovies(piece);
        return Ok(legalMoves);
    }
}