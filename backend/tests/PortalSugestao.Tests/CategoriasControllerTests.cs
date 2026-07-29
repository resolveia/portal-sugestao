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
}
