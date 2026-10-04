using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuditDemo.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdminController(DemoDbContext dbContext) : ControllerBase
{
    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await dbContext.Customers
            .Select(customer => new
            {
                customer.Id,
                customer.Email,
                customer.FullName
            })
            .ToListAsync();

        return Ok(users);
    }
}
