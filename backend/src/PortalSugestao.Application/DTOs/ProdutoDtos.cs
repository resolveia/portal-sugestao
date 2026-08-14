namespace PortalSugestao.Application.DTOs;

public record ProdutoDto(int Id, string Nome, bool Ativo);

public record CreateProdutoRequest(string Nome);

public record ListarProdutosResponse(bool Erro, string? Mensagem, IReadOnlyList<ProdutoDto>? Produtos);

public record ProdutoResponse(bool Erro, string? Mensagem, ProdutoDto? Produto);
