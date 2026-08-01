using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Application.DTOs;

public record SugestaoDto(
    int Id,
    string Titulo,
    string Descricao,
    string ResultadoEsperado,
    int ProdutoId,
    string ProdutoNome,
    int CategoriaId,
    string CategoriaNome,
    int AutorId,
    string AutorNome,
    StatusSugestao Status,
    DateTime DataCriacao,
    int TotalVotos,
    bool VotadoPorMim,
    DateTime? DataModeracao = null,
    string? MotivoRejeicao = null,
    string? ModeradorNome = null);

public record CreateSugestaoRequest(int ProdutoId, string Titulo, string Descricao, string ResultadoEsperado, int CategoriaId);

/// <summary>
/// Resposta paginada do ranking — evita transferir/materializar todas as sugestões publicadas de uma vez
/// (RNF seção 11, ver docs/performance-report.md). VotosUsadosPeloUsuarioAtual vem à parte porque o
/// limite de 3 votos ativos (regra 7.2) é sobre o total do usuário, não sobre a página atual.
/// </summary>
public record SugestoesPaginadasDto(IReadOnlyList<SugestaoDto> Items, int Total, int VotosUsadosPeloUsuarioAtual);

public record RejeitarSugestaoRequest(string Motivo);
