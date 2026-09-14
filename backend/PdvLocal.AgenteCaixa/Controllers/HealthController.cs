using Microsoft.AspNetCore.Mvc;

namespace PdvLocal.AgenteCaixa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "ok", agente = "Agente Caixa Local", timestamp = DateTime.Now });
    }

    
}
