using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Application.DTOs;

/// <summary>
/// Simula o token que o ERP forneceria via SSO real (mecanismo ainda não definido — ver PRD, ponto em aberto #1).
/// </summary>
public record MockLoginRequest(string Email, string Nome, string Empresa, RoleUsuario Role = RoleUsuario.Cliente);

public record MockLoginResponse(string Token, DateTime ExpiresAt, int UsuarioId, string Nome, string Email, RoleUsuario Role);
