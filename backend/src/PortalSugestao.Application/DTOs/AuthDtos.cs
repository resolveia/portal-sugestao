using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Application.DTOs;

/// <summary>
/// Dados devolvidos pelo login real contra a api_authentication (endpoint /api/authentication/logar,
/// ver docs/autenticacao-e-api-portal-sugestoes.md) que o front repassa pra essa API estabelecer a
/// sessão local. Nesta aplicação local o login é sempre considerado válido — quem valida usuário/senha
/// de verdade é a api_authentication real; aqui só espelhamos os dados que ela devolveria.
/// </summary>
public record SessaoRequest(string Nome, string Login, int Id, string EmpresaId, bool AdminPortalSugestoes);

public record UsuarioLogadoDto(int Id, string Nome, string Email, RoleUsuario Role);

/// <summary>Envelope padrão da plataforma: sempre HTTP 200, erro sinalizado pelo campo Erro.</summary>
public record SessaoResponse(bool Erro, string? Mensagem, UsuarioLogadoDto? Usuario);

public record LogoutResponse(bool Erro);
