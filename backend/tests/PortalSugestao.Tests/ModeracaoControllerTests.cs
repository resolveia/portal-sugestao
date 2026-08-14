using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class ModeracaoControllerTests
{
    [Fact]
    public async Task Pendentes_SoAdmin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync("/api/sugestoes/pendentes", new { });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Aprovar_PublicaERegistraAuditoria()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sugestao = body.GetProperty("Sugestao");

        Assert.Equal("Publicada", sugestao.GetProperty("Status").GetString());
        Assert.Equal("Admin", sugestao.GetProperty("ModeradorNome").GetString());
        Assert.False(string.IsNullOrEmpty(sugestao.GetProperty("DataModeracao").GetString()));
    }

    [Fact]
    public async Task Rejeitar_GravaMotivo()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await admin.PostAsJsonAsync($"/api/sugestoes/rejeitar/{id}", new { motivo = "Duplicada" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sugestao = body.GetProperty("Sugestao");

        Assert.Equal("Rejeitada", sugestao.GetProperty("Status").GetString());
        Assert.Equal("Duplicada", sugestao.GetProperty("MotivoRejeicao").GetString());
    }

    [Fact]
    public async Task ModerarDuasVezes_DevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
        var segunda = await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
        var body = await segunda.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(body.GetProperty("Erro").GetBoolean());
    }
}
