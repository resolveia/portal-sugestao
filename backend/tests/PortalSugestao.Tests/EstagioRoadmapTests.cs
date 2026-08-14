using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class EstagioRoadmapTests
{
    private static async Task<int> CriarEPublicarAsync(HttpClient admin, HttpClient autor, int categoriaId, string titulo)
    {
        var id = await autor.CriarSugestaoAsync(categoriaId, titulo);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
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

        var response = await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Planejado" });
        Assert.False((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var ranking = await (await cliente.PostAsJsonAsync("/api/sugestoes/listar", new { Skip = 0, Take = 20 })).Content.ReadFromJsonAsync<JsonElement>();
        var item = ranking.GetProperty("Sugestoes").EnumerateArray().First(s => s.GetProperty("Id").GetInt32() == id);
        Assert.Equal("Planejado", item.GetProperty("EstagioRoadmap").GetString());
    }

    [Fact]
    public async Task Cliente_NaoConsegueDefinirEstagio()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao roadmap 2");

        var response = await cliente.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Planejado" });
        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task NaoConsegueDefinirEstagio_DeSugestaoAindaEmModeracao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId, "Sugestao roadmap 3 (nao publicada)");

        var response = await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "EmAnalise" });
        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Votar_EmSugestaoLancada_DevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.roadmap4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.roadmap4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao roadmap 4");

        await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Lancado" });

        var response = await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id}", new { });
        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }
}
