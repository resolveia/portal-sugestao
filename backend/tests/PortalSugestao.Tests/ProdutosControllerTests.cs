using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class ProdutosControllerTests
{
    [Fact]
    public async Task Admin_ConsegueCriarProduto()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod1@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/produtos", new { nome = "AJORS.CRM" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Cliente_NaoConsegueCriarProduto()
    {
        await using var factory = new CustomWebApplicationFactory();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.prod1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync("/api/produtos", new { nome = "AJORS.CRM" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Listar_JaTrazOsProdutosSemeados()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod2@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.GetAsync("/api/produtos");
        var produtos = await response.Content.ReadFromJsonAsync<JsonElement>();
        var nomes = produtos.EnumerateArray().Select(p => p.GetProperty("nome").GetString()).ToList();

        Assert.Contains("AJORS.OOH", nomes);
        Assert.Contains("AJORS.SIGN", nomes);
    }

    [Fact]
    public async Task Criar_NomeVazioDaBadRequest()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod3@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/produtos", new { nome = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Editar_Renomeia()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("Nome Antigo");

        var response = await admin.PutAsJsonAsync($"/api/produtos/{id}", new { nome = "Nome Novo" });
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Nome Novo", dto.GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueEditarOuRemover()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("AJORS.CRM");
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.prod5@empresa.com", "Cliente", "Empresa", "Cliente");

        var respostaEditar = await cliente.PutAsJsonAsync($"/api/produtos/{id}", new { nome = "Hack" });
        var respostaRemover = await cliente.DeleteAsync($"/api/produtos/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, respostaEditar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, respostaRemover.StatusCode);
    }

    [Fact]
    public async Task Remover_DesativaESomeDaListaAtivos_MasContinuaEmTodos()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod6@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("Descontinuado");

        var respostaRemover = await admin.DeleteAsync($"/api/produtos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, respostaRemover.StatusCode);

        var ativos = await (await admin.GetAsync("/api/produtos")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.DoesNotContain(ativos.EnumerateArray(), p => p.GetProperty("id").GetInt32() == id);

        var todos = await (await admin.GetAsync("/api/produtos/todos")).Content.ReadFromJsonAsync<JsonElement>();
        var produto = todos.EnumerateArray().Single(p => p.GetProperty("id").GetInt32() == id);
        Assert.False(produto.GetProperty("ativo").GetBoolean());
    }
}
