namespace PortalSugestao.Application.DTOs;

public record CategoriaDto(int Id, string Nome, bool Ativo);

public record CreateCategoriaRequest(string Nome);

public record ListarCategoriasResponse(bool Erro, string? Mensagem, IReadOnlyList<CategoriaDto>? Categorias);

public record CategoriaResponse(bool Erro, string? Mensagem, CategoriaDto? Categoria);
