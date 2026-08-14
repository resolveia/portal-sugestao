using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class ComentariosControllerTests
{
    [Fact]
    public async Task ClienteEAdmin_ConseguemComentarEmSugestaoPublicada()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.com1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.com1@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        var respostaCliente = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Comentario cliente" });
        var respostaAdmin = await admin.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Comentario admin" });

        Assert.False((await respostaCliente.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
        Assert.False((await respostaAdmin.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var lista = await (await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/listar", new { })).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, lista.GetProperty("Comentarios").GetArrayLength());
    }

    [Fact]
    public async Task Comentar_EmSugestaoNaoPublicadaDevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.com2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.com2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Nao deveria funcionar" });

        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Remover_SoAdmin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.com3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.com3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        var criado = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Comentario" });
        var comentarioId = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Comentario").GetProperty("Id").GetInt32();

        var tentativaCliente = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/remover/{comentarioId}", new { });
        Assert.True((await tentativaCliente.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var tentativaAdmin = await admin.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/remover/{comentarioId}", new { });
        Assert.False((await tentativaAdmin.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }
}
