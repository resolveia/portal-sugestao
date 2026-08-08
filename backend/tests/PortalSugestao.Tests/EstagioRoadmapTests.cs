using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class EstagioRoadmapTests
{
    private static async Task<int> CriarEPublicarAsync(HttpClient admin, HttpClient autor, int categoriaId, string titulo)
    {
        var id = await autor.CriarSugestaoAsync(categoriaId, titulo);
        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);
        return id;
    }

    [Fact]
    public async Task Admin_ConsegueDefinirEstagio_EApareceNoRanking()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap1@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao roadmap 1");

        var response = await admin.PutAsJsonAsync($"/api/sugestoes/{id}/roadmap", new { estagio = "Planejado" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ranking = await (await cliente.GetAsync("/api/sugestoes")).Content.ReadFromJsonAsync<JsonElement>();
        var item = ranking.GetProperty("items").EnumerateArray().First(s => s.GetProperty("id").GetInt32() == id);
        Assert.Equal("Planejado", item.GetProperty("estagioRoadmap").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueDefinirEstagio()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao roadmap 2");

        var response = await cliente.PutAsJsonAsync($"/api/sugestoes/{id}/roadmap", new { estagio = "Planejado" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NaoConsegueDefinirEstagio_DeSugestaoAindaEmModeracao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId, "Sugestao roadmap 3 (nao publicada)");

        var response = await admin.PutAsJsonAsync($"/api/sugestoes/{id}/roadmap", new { estagio = "EmAnalise" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Votar_EmSugestaoLancada_DaConflito()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao roadmap 4");

        await admin.PutAsJsonAsync($"/api/sugestoes/{id}/roadmap", new { estagio = "Lancado" });

        var response = await cliente.PostAsync($"/api/sugestoes/{id}/votos", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
