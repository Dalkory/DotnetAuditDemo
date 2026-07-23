using Microsoft.AspNetCore.Mvc;

namespace DotnetAuditDemo.Api.Controllers;

[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "running",
            checkedAtUtc = DateTime.UtcNow
        });
    }
}
