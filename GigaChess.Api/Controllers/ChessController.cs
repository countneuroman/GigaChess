using Microsoft.AspNetCore.Mvc;

namespace GigaChess.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ChessController : Controller
{
    [HttpPost]
    public async Task<IActionResult> RandomMove()
    {
        
        return Ok("Good");
    }
}