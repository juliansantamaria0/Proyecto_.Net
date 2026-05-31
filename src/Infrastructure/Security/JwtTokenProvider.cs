using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AutoTallerManager.Infrastructure.Security;

public class JwtTokenProvider(IConfiguration configuration) : IJwtTokenProvider
{
    public string GenerateToken(Usuario usuario, DateTime expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("UserId", usuario.Id.ToString()),
        };

        if (usuario.ClienteId.HasValue)
            claims.Add(new Claim("ClienteId", usuario.ClienteId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetExpirationHours() =>
        int.TryParse(configuration["Jwt:ExpirationHours"], out var hours) ? hours : 8;
}
