namespace PortalSugestao.Application.DTOs;

public record ProdutoDto(int Id, string Nome, bool Ativo);

public record CreateProdutoRequest(string Nome);
