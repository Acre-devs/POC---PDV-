using Microsoft.AspNetCore.Mvc;

namespace PdvLocal.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "ok", servidor = "Servidor Central Loja", timestamp = DateTime.Now });
    }
}
