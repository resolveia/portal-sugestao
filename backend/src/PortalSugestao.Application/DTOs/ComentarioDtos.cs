namespace PortalSugestao.Application.DTOs;

public record ComentarioDto(int Id, int SugestaoId, int UsuarioId, string AutorNome, string Texto, DateTime DataCriacao);

public record CreateComentarioRequest(string Texto);
