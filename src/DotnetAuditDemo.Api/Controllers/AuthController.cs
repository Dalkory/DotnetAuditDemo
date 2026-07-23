using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAuditDemo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    private static readonly Action<ILogger, string, string, Exception?> LogLoginRequested =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(2001, nameof(CreateToken)),
            "Login requested by {Email} with password {Password}");

    [HttpPost("token")]
    public IActionResult CreateToken(LoginRequest request)
    {
        // Demo-only authentication. Deliberate defect: credentials and PII are logged.
        LogLoginRequested(logger, request.Email, request.Password, null);

        var key = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is missing.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "DotnetAuditDemo",
            audience: "DotnetAuditDemo.Client",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, request.Email),
                new Claim(ClaimTypes.Name, request.Email)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }

    public sealed record LoginRequest(string Email, string Password);
}
