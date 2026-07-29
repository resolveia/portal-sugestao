using System.Net;
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
        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);

        var respostaCliente = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios", new { texto = "Comentario cliente" });
        var respostaAdmin = await admin.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios", new { texto = "Comentario admin" });

        Assert.Equal(HttpStatusCode.Created, respostaCliente.StatusCode);
        Assert.Equal(HttpStatusCode.Created, respostaAdmin.StatusCode);

        var lista = await (await cliente.GetAsync($"/api/sugestoes/{id}/comentarios")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, lista.GetArrayLength());
    }

    [Fact]
    public async Task Comentar_EmSugestaoNaoPublicadaDaConflito()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.com2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.com2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios", new { texto = "Nao deveria funcionar" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Remover_SoAdmin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.com3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.com3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);
        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);

        var criado = await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios", new { texto = "Comentario" });
        var comentarioId = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var tentativaCliente = await cliente.DeleteAsync($"/api/sugestoes/{id}/comentarios/{comentarioId}");
        Assert.Equal(HttpStatusCode.Forbidden, tentativaCliente.StatusCode);

        var tentativaAdmin = await admin.DeleteAsync($"/api/sugestoes/{id}/comentarios/{comentarioId}");
        Assert.Equal(HttpStatusCode.NoContent, tentativaAdmin.StatusCode);
    }
}
