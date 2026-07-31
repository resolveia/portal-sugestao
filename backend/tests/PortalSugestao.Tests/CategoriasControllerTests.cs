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

        var response = await admin.PostAsJsonAsync("/api/categorias", new { nome = "Financeiro" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Cliente_NaoConsegueCriarCategoria()
    {
        await using var factory = new CustomWebApplicationFactory();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.cat1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync("/api/categorias", new { nome = "Financeiro" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Listar_RetornaCategoriaCriada()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat2@empresa.com", "Admin", "Empresa", "AdminInterno");
        await admin.CriarCategoriaAsync("Financeiro");

        var response = await admin.GetAsync("/api/categorias");
        var categorias = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains(categorias.EnumerateArray(), c => c.GetProperty("nome").GetString() == "Financeiro");
    }

    [Fact]
    public async Task Criar_NomeVazioDaBadRequest()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat3@empresa.com", "Admin", "Empresa", "AdminInterno");

        var response = await admin.PostAsJsonAsync("/api/categorias", new { nome = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Editar_Renomeia()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Nome Antigo");

        var response = await admin.PutAsJsonAsync($"/api/categorias/{id}", new { nome = "Nome Novo" });
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Nome Novo", dto.GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueEditarOuRemover()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Financeiro");
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.cat5@empresa.com", "Cliente", "Empresa", "Cliente");

        var respostaEditar = await cliente.PutAsJsonAsync($"/api/categorias/{id}", new { nome = "Hack" });
        var respostaRemover = await cliente.DeleteAsync($"/api/categorias/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, respostaEditar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, respostaRemover.StatusCode);
    }

    [Fact]
    public async Task Remover_DesativaESomeDaListaAtivas_MasContinuaEmTodas()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.cat6@empresa.com", "Admin", "Empresa", "AdminInterno");
        var id = await admin.CriarCategoriaAsync("Descontinuada");

        var respostaRemover = await admin.DeleteAsync($"/api/categorias/{id}");
        Assert.Equal(HttpStatusCode.NoContent, respostaRemover.StatusCode);

        var ativas = await (await admin.GetAsync("/api/categorias")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.DoesNotContain(ativas.EnumerateArray(), c => c.GetProperty("id").GetInt32() == id);

        var todas = await (await admin.GetAsync("/api/categorias/todas")).Content.ReadFromJsonAsync<JsonElement>();
        var categoria = todas.EnumerateArray().Single(c => c.GetProperty("id").GetInt32() == id);
        Assert.False(categoria.GetProperty("ativo").GetBoolean());
    }
}
