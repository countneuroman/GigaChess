using GigaChess.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GigaChess.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ChessController : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(Move), 200)]
    public async Task<IActionResult> RandomMove()
    {
        var move = new Move() { From = "e2", To = "e4" };

        return Ok(move);
    }
}