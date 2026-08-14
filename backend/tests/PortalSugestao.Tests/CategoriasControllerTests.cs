using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class CategoriasControllerTests
{
    [Fact]
    public async Task Admin_ConsegueCriarCategoria()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat1@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/categorias/salvar", new { nome = "Financeiro" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Cliente_NaoConsegueCriarCategoria()
    {
        await using var factory = new CustomWebApplicationFactory();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.cat1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync("/api/categorias/salvar", new { nome = "Financeiro" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Listar_RetornaCategoriaCriada()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat2@empresa.com", "Admin", "Empresa", "AdminInterno");
        await admin.CriarCategoriaAsync("Financeiro");

        var response = await admin.PostAsJsonAsync("/api/categorias/listar", new { });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var categorias = body.GetProperty("Categorias");

        Assert.Contains(categorias.EnumerateArray(), c => c.GetProperty("Nome").GetString() == "Financeiro");
    }

    [Fact]
    public async Task Salvar_NomeVazioDevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat3@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/categorias/salvar", new { nome = "  " });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Editar_Renomeia()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Nome Antigo");

        var response = await admin.PostAsJsonAsync($"/api/categorias/editar/{id}", new { nome = "Nome Novo" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(body.GetProperty("Erro").GetBoolean());
        Assert.Equal("Nome Novo", body.GetProperty("Categoria").GetProperty("Nome").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueEditarOuRemover()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Financeiro");
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.cat5@empresa.com", "Cliente", "Empresa", "Cliente");

        var respostaEditar = await cliente.PostAsJsonAsync($"/api/categorias/editar/{id}", new { nome = "Hack" });
        var respostaRemover = await cliente.PostAsJsonAsync($"/api/categorias/remover/{id}", new { });

        Assert.True((await respostaEditar.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
        Assert.True((await respostaRemover.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Remover_DesativaESomeDaListaAtivas_MasContinuaEmTodas()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat6@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Descontinuada");

        var respostaRemover = await admin.PostAsJsonAsync($"/api/categorias/remover/{id}", new { });
        var corpoRemover = await respostaRemover.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(corpoRemover.GetProperty("Erro").GetBoolean());

        var ativas = (await (await admin.PostAsJsonAsync("/api/categorias/listar", new { })).Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("Categorias");
        Assert.DoesNotContain(ativas.EnumerateArray(), c => c.GetProperty("Id").GetInt32() == id);

        var todas = (await (await admin.PostAsJsonAsync("/api/categorias/listartodas", new { })).Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("Categorias");
        var categoria = todas.EnumerateArray().Single(c => c.GetProperty("Id").GetInt32() == id);
        Assert.False(categoria.GetProperty("Ativo").GetBoolean());
    }
}
