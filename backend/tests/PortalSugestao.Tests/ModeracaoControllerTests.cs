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

        var response = await cliente.GetAsync("/api/sugestoes/pendentes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Aprovar_PublicaERegistraAuditoria()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Publicada", dto.GetProperty("status").GetString());
        Assert.Equal("Admin", dto.GetProperty("moderadorNome").GetString());
        Assert.False(string.IsNullOrEmpty(dto.GetProperty("dataModeracao").GetString()));
    }

    [Fact]
    public async Task Rejeitar_GravaMotivo()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        var response = await admin.PutAsJsonAsync($"/api/sugestoes/{id}/rejeitar", new { motivo = "Duplicada" });
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Rejeitada", dto.GetProperty("status").GetString());
        Assert.Equal("Duplicada", dto.GetProperty("motivoRejeicao").GetString());
    }

    [Fact]
    public async Task ModerarDuasVezes_DaConflito()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.mod4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.mod4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);
        var segunda = await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }
}
