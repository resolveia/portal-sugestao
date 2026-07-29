using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Application.DTOs;

public record SugestaoDto(
    int Id,
    string Titulo,
    string Descricao,
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

public record CreateSugestaoRequest(string Titulo, string Descricao, int CategoriaId);

public record RejeitarSugestaoRequest(string Motivo);
