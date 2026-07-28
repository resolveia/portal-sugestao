namespace PortalSugestao.Application.DTOs;

public record CategoriaDto(int Id, string Nome, bool Ativo);

public record CreateCategoriaRequest(string Nome);
