using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Services;
using Microsoft.IdentityModel.Tokens;

namespace MediCore.Api.Infrastructure;

// Costruisce e firma il token JWT di accesso a partire dalla configurazione.
public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;

    public TokenResult CreateToken(AppUser user, IList<string> roles)
    {
        var jwt = _configuration.GetSection("Jwt");
        var key = jwt["Key"]
            ?? throw new InvalidOperationException("Chiave JWT non configurata (Jwt:Key).");
        var expiryMinutes = int.TryParse(jwt["ExpiryMinutes"], out var minutes) ? minutes : 120;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(tokenString, expiresAtUtc);
    }
}
