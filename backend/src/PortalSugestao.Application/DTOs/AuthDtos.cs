using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Application.DTOs;

/// <summary>
/// Simula o token que o ERP forneceria via SSO real (mecanismo ainda não definido — ver PRD, ponto em aberto #1).
/// </summary>
public record MockLoginRequest(string Email, string Nome, string Empresa, RoleUsuario Role = RoleUsuario.Cliente);

public record MockLoginResponse(string Token, DateTime ExpiresAt, int UsuarioId, string Nome, string Email, RoleUsuario Role);

/// <summary>Payload recebido na rota de login automático (equivalente ao "TokenAcesso" do fluxo real do ERP).</summary>
public record LoginTokenRequest(string Token);

public record UsuarioLogadoDto(int Id, string Nome, string Email, RoleUsuario Role);

/// <summary>
/// Sempre devolvido com 200 OK (mesmo em erro de token) — padrão definido pelo time do ERP
/// para o fluxo de login direto (ver docs/sso-checklist.md).
/// </summary>
public record LoginTokenResponse(bool Erro, string? Mensagem, UsuarioLogadoDto? Usuario);

public record TokensDemoResponse(string Admin, string Cliente);
