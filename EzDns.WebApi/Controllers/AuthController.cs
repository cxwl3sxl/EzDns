using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EzDns.WebApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EzDns.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IOptions<AuthOptions> options) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var cfg = options.Value;

        if (!string.Equals(request.Username, cfg.Username, StringComparison.Ordinal) ||
            !string.Equals(request.Password, cfg.Password, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        var jwtCfg = cfg.Jwt;
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg.Secret));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  request.Username),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,              request.Username),
            new Claim(ClaimTypes.Role,              "Administrator"),
        };

        var token = new JwtSecurityToken(
            issuer:   jwtCfg.Issuer,
            audience: jwtCfg.Audience,
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(jwtCfg.ExpiryMinutes),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token     = tokenString,
            expiresAt = token.ValidTo,
            username  = request.Username,
        });
    }
}

public record LoginRequest(string Username, string Password);
