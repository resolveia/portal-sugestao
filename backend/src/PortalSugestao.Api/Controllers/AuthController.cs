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
    private readonly ErpTokenSimuladoService _erpTokenService;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        PortalSugestaoDbContext db,
        MockTokenService tokenService,
        ErpTokenSimuladoService erpTokenService,
        IWebHostEnvironment env)
    {
        _db = db;
        _tokenService = tokenService;
        _erpTokenService = erpTokenService;
        _env = env;
    }

    /// <summary>
    /// Login manual (formulário): recebe os dados básicos do usuário e devolve um JWT.
    /// Continua existindo em paralelo ao login automático via token (rota "login-token") —
    /// decisão do time do ERP (2026-08-12, ver docs/sso-checklist.md).
    /// </summary>
    [HttpPost("mock-login")]
    public async Task<ActionResult<MockLoginResponse>> MockLogin(MockLoginRequest request)
    {
        var usuario = await ObterOuCriarUsuarioAsync(
            erpUserId: $"mock:{request.Email}",
            nome: request.Nome,
            email: request.Email,
            empresa: request.Empresa,
            role: request.Role);

        var (token, expiresAt) = _tokenService.GenerateToken(usuario);
        DefinirCookieSessao(token, expiresAt);

        return Ok(new MockLoginResponse(token, expiresAt, usuario.Id, usuario.Nome, usuario.Email, usuario.Role));
    }

    /// <summary>
    /// Login automático via token (equivalente à rota que o ERP vai chamar passando "?token=..." —
    /// ver docs/sso-checklist.md). O token hoje é simulado (<see cref="ErpTokenSimuladoService"/>);
    /// será substituído pela decriptação real assim que o time do ERP definir o algoritmo/chave
    /// (PRD, ponto em aberto #1). Sempre responde 200 OK — erro vem no campo "erro", conforme
    /// padrão definido pelo time do ERP.
    /// </summary>
    [HttpPost("login-token")]
    public async Task<ActionResult<LoginTokenResponse>> LoginToken(LoginTokenRequest request)
    {
        var payload = _erpTokenService.Decodificar(request.Token);
        if (payload is null)
        {
            return Ok(new LoginTokenResponse(true, "Token inválido ou expirado.", null));
        }

        var usuario = await ObterOuCriarUsuarioAsync(
            erpUserId: payload.ErpUserId,
            nome: payload.Nome,
            email: payload.Email,
            empresa: payload.Empresa,
            role: payload.Role);

        var (token, expiresAt) = _tokenService.GenerateToken(usuario);
        DefinirCookieSessao(token, expiresAt);

        return Ok(new LoginTokenResponse(false, null, new UsuarioLogadoDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role)));
    }

    /// <summary>
    /// Encerra a sessão: o cookie é HttpOnly, então só o backend consegue removê-lo de fato
    /// (o front não tem acesso a ele via JS pra apagar sozinho).
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieDefaults.Name, new CookieOptions { Path = "/" });
        return Ok();
    }

    /// <summary>
    /// Gera tokens de demonstração (Admin e Cliente) só pra simular, em desenvolvimento, o link que
    /// o ERP abriria com "?token=...". Remover quando o token real do ERP existir.
    /// </summary>
    [HttpGet("tokens-demo")]
    public ActionResult<TokensDemoResponse> TokensDemo()
    {
        return Ok(new TokensDemoResponse(
            Admin: _erpTokenService.GerarToken(ErpTokenSimuladoService.AdminDemo),
            Cliente: _erpTokenService.GerarToken(ErpTokenSimuladoService.ClienteDemo)));
    }

    private async Task<Usuario> ObterOuCriarUsuarioAsync(string erpUserId, string nome, string email, string empresa, RoleUsuario role)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ErpUserId == erpUserId);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Nome = nome,
                Email = email,
                Empresa = empresa,
                ErpUserId = erpUserId,
                Role = role
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }

        return usuario;
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
