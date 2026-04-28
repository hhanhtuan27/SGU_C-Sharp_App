using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VinhKhanhAdmin.Models;

namespace VinhKhanhAdmin.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string CreateToken(User user)
    {
        var key   = _config["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer= _config["Jwt:Issuer"]   ?? "VinhKhanhAdmin";
        var aud   = _config["Jwt:Audience"] ?? "VinhKhanhApp";
        var days  = _config.GetValue("Jwt:ExpiresInDays", 30);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer, audience: aud,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(days),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
