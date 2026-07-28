using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Api.Auth;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;
    private readonly MockTokenService _tokenService;

    public AuthController(PortalSugestaoDbContext db, MockTokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Simula o login via SSO do ERP: recebe os dados básicos do usuário e devolve um JWT.
    /// Mecanismo temporário — será substituído pela validação do token real do ERP (PRD, ponto em aberto #1).
    /// </summary>
    [HttpPost("mock-login")]
    public async Task<ActionResult<MockLoginResponse>> MockLogin(MockLoginRequest request)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                Empresa = request.Empresa,
                ErpUserId = $"mock:{request.Email}",
                Role = request.Role
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }

        var (token, expiresAt) = _tokenService.GenerateToken(usuario);

        return Ok(new MockLoginResponse(token, expiresAt, usuario.Id, usuario.Nome, usuario.Email, usuario.Role));
    }
}
