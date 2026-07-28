using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortalSugestao.Domain.Entities;

namespace PortalSugestao.Api.Auth;

/// <summary>
/// Emite o JWT que simula o token de SSO do ERP (mecanismo real ainda a definir — PRD, ponto em aberto #1).
/// Substituir por validação do token real do ERP quando o mecanismo for definido.
/// </summary>
public class MockTokenService
{
    private readonly MockAuthOptions _options;

    public MockTokenService(IOptions<MockAuthOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(Usuario usuario)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.TokenExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("empresa", usuario.Empresa),
            new Claim(ClaimTypes.Role, usuario.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
