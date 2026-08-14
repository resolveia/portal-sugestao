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

        var response = await admin.PostAsJsonAsync("/api/produtos/salvar", new { nome = "AJORS.CRM" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Cliente_NaoConsegueCriarProduto()
    {
        await using var factory = new CustomWebApplicationFactory();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.prod1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync("/api/produtos/salvar", new { nome = "AJORS.CRM" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Listar_JaTrazOsProdutosSemeados()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod2@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/produtos/listar", new { });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var nomes = body.GetProperty("Produtos").EnumerateArray().Select(p => p.GetProperty("Nome").GetString()).ToList();

        Assert.Contains("AJORS.OOH", nomes);
        Assert.Contains("AJORS.SIGN", nomes);
    }

    [Fact]
    public async Task Salvar_NomeVazioDevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod3@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/produtos/salvar", new { nome = "  " });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Editar_Renomeia()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("Nome Antigo");

        var response = await admin.PostAsJsonAsync($"/api/produtos/editar/{id}", new { nome = "Nome Novo" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(body.GetProperty("Erro").GetBoolean());
        Assert.Equal("Nome Novo", body.GetProperty("Produto").GetProperty("Nome").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueEditarOuRemover()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("AJORS.CRM");
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.prod5@empresa.com", "Cliente", "Empresa", "Cliente");

        var respostaEditar = await cliente.PostAsJsonAsync($"/api/produtos/editar/{id}", new { nome = "Hack" });
        var respostaRemover = await cliente.PostAsJsonAsync($"/api/produtos/remover/{id}", new { });

        Assert.True((await respostaEditar.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
        Assert.True((await respostaRemover.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Remover_DesativaESomeDaListaAtivos_MasContinuaEmTodos()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.prod6@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarProdutoAsync("Descontinuado");

        var respostaRemover = await admin.PostAsJsonAsync($"/api/produtos/remover/{id}", new { });
        var corpoRemover = await respostaRemover.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(corpoRemover.GetProperty("Erro").GetBoolean());

        var ativos = (await (await admin.PostAsJsonAsync("/api/produtos/listar", new { })).Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("Produtos");
        Assert.DoesNotContain(ativos.EnumerateArray(), p => p.GetProperty("Id").GetInt32() == id);

        var todos = (await (await admin.PostAsJsonAsync("/api/produtos/listartodos", new { })).Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("Produtos");
        var produto = todos.EnumerateArray().Single(p => p.GetProperty("Id").GetInt32() == id);
        Assert.False(produto.GetProperty("Ativo").GetBoolean());
    }
}
