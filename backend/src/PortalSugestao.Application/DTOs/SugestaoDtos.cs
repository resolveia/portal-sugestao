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
    EstagioRoadmap? EstagioRoadmap,
    DateTime DataCriacao,
    int TotalVotos,
    bool VotadoPorMim,
    DateTime? DataModeracao = null,
    string? MotivoRejeicao = null,
    string? ModeradorNome = null);

public record CreateSugestaoRequest(int ProdutoId, string Titulo, string Descricao, string ResultadoEsperado, int CategoriaId);

public record AtualizarEstagioRoadmapRequest(EstagioRoadmap Estagio);

public record RejeitarSugestaoRequest(string Motivo);

public record ListarSugestoesRequest(int Skip = 0, int Take = 20);

/// <summary>
/// Resposta paginada do ranking — evita transferir/materializar todas as sugestões publicadas de uma vez
/// (RNF seção 11, ver docs/performance-report.md). VotosUsadosPeloUsuarioAtual vem à parte porque o
/// limite de 3 votos ativos (regra 7.2) é sobre o total do usuário, não sobre a página atual.
/// </summary>
public record ListarSugestoesResponse(
    bool Erro,
    string? Mensagem,
    IReadOnlyList<SugestaoDto>? Sugestoes,
    int Total,
    int VotosUsadosPeloUsuarioAtual);

public record PendentesResponse(bool Erro, string? Mensagem, IReadOnlyList<SugestaoDto>? Sugestoes);

public record SugestaoResponse(bool Erro, string? Mensagem, SugestaoDto? Sugestao);
