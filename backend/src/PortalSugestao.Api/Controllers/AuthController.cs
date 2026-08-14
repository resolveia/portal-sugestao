using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Api.Auth;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;
    private readonly MockTokenService _tokenService;
    private readonly IWebHostEnvironment _env;

    public AuthController(PortalSugestaoDbContext db, MockTokenService tokenService, IWebHostEnvironment env)
    {
        _db = db;
        _tokenService = tokenService;
        _env = env;
    }

    /// <summary>
    /// Estabelece a sessão local do Portal depois que o front já logou de verdade contra a
    /// api_authentication (ver docs/autenticacao-e-api-portal-sugestoes.md) — o login em si não é
    /// feito aqui. Nesta aplicação local o login é sempre considerado válido (decisão do usuário,
    /// 2026-08-14): não há validação de credencial nem fidelidade ao schema real do cookie/JWT,
    /// só criação/atualização do Usuario local e emissão do cookie de sessão que protege as
    /// rotas Admin deste "dublê" de api_portal_sugestoes.
    /// A api_portal_sugestoes real cria o usuário sozinha no primeiro login — aqui replicamos
    /// esse comportamento localmente, com dados derivados quando a resposta da api_authentication
    /// não os fornece (ela não devolve e-mail nem nome de empresa, só Login/EmpresaId).
    /// </summary>
    [HttpPost("sessao")]
    public async Task<ActionResult<SessaoResponse>> Sessao(SessaoRequest request)
    {
        var erpUserId = $"{request.EmpresaId}:{request.Id}";
        var role = request.AdminPortalSugestoes ? RoleUsuario.AdminInterno : RoleUsuario.Cliente;

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ErpUserId == erpUserId);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Nome = request.Nome,
                Email = $"{request.Login}@erp.local", // placeholder — api_authentication real não devolve e-mail
                Empresa = request.EmpresaId, // placeholder — resposta de login não traz o nome da empresa
                ErpUserId = erpUserId,
                Role = role
            };
            _db.Usuarios.Add(usuario);
        }
        else
        {
            usuario.Nome = request.Nome;
            usuario.Role = role; // a role é devolvida a cada login (decisão do usuário, 2026-08-14)
        }

        await _db.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.GenerateToken(usuario);
        DefinirCookieSessao(token, expiresAt);

        return Ok(new SessaoResponse(false, null, new UsuarioLogadoDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role)));
    }

    /// <summary>
    /// Encerra a sessão local. Só o backend consegue apagar o cookie (HttpOnly, não acessível via JS).
    /// Sem correspondência com a api_authentication real: lá, o logout é POST /api/authentication/logout.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieDefaults.Name, new CookieOptions { Path = "/" });
        return Ok(new LogoutResponse(false));
    }

    private void DefinirCookieSessao(string token, DateTime expiresAt)
    {
        Response.Cookies.Append(AuthCookieDefaults.Name, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !_env.IsDevelopment(),
            Expires = expiresAt,
            Path = "/"
        });
    }
}
