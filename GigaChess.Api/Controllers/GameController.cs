using GigaChess.Api.Models;
using GigaChess.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GigaChess.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("New")]
    public IActionResult NewGame()
    {
        var response = _gameService.CreateGame();
        return Ok(response);
    }

    [HttpGet("{gameId}")]
    public IActionResult GetGame(string gameId)
    {
        var response = _gameService.GetGame(gameId);
        if (response == null)
            return NotFound(new { Error = "Game not found." });

        return Ok(response);
    }

    [HttpPost("LegalMoves")]
    public IActionResult GetLegalMoves([FromBody] GetLegalMovesRequest request)
    {
        var result = _gameService.GetLegalMoves(request);

        if (result.IsNotFound)
            return NotFound(new { result.Error });

        if (!result.Success)
            return BadRequest(new { result.Error });

        return Ok(result.Data);
    }

    [HttpPost("Move")]
    public IActionResult MakeMove([FromBody] MakeMoveRequest request)
    {
        var result = _gameService.MakeMove(request);

        if (result.IsNotFound)
            return NotFound(new { result.Error });

        if (!result.Success)
            return BadRequest(new { result.Error });

        return Ok(result.Data);
    }
}
