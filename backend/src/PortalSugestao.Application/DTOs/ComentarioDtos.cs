namespace PortalSugestao.Application.DTOs;

public record ComentarioDto(int Id, int SugestaoId, int UsuarioId, string AutorNome, string Texto, DateTime DataCriacao);

public record CreateComentarioRequest(string Texto);

public record ListarComentariosResponse(bool Erro, string? Mensagem, IReadOnlyList<ComentarioDto>? Comentarios);

public record ComentarioResponse(bool Erro, string? Mensagem, ComentarioDto? Comentario);

public record RemoverComentarioResponse(bool Erro, string? Mensagem);
