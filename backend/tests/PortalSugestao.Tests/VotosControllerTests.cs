using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class VotosControllerTests
{
    private static async Task<int> CriarEPublicarAsync(HttpClient admin, HttpClient autor, int categoriaId, string titulo)
    {
        var id = await autor.CriarSugestaoAsync(categoriaId, titulo);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
        return id;
    }

    [Fact]
    public async Task Votar_RegistraVotoEAparaceNoRanking()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot1@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao 1");

        var response = await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id}", new { });
        Assert.False((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var ranking = await (await cliente.PostAsJsonAsync("/api/sugestoes/listar", new { Skip = 0, Take = 20 })).Content.ReadFromJsonAsync<JsonElement>();
        var item = ranking.GetProperty("Sugestoes").EnumerateArray().First(s => s.GetProperty("Id").GetInt32() == id);

        Assert.Equal(1, item.GetProperty("TotalVotos").GetInt32());
        Assert.True(item.GetProperty("VotadoPorMim").GetBoolean());
        Assert.Equal(1, ranking.GetProperty("VotosUsadosPeloUsuarioAtual").GetInt32());
    }

    [Fact]
    public async Task Votar_DuasVezesNaMesmaSugestaoDevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao 1");

        await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id}", new { });
        var segundo = await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id}", new { });

        Assert.True((await segundo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Votar_LimiteDeTresAtivosComRealocacao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot3@empresa.com", "Cliente", "Empresa", "Cliente");

        var id1 = await CriarEPublicarAsync(admin, cliente, categoriaId, "S1");
        var id2 = await CriarEPublicarAsync(admin, cliente, categoriaId, "S2");
        var id3 = await CriarEPublicarAsync(admin, cliente, categoriaId, "S3");
        var id4 = await CriarEPublicarAsync(admin, cliente, categoriaId, "S4");

        await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id1}", new { });
        await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id2}", new { });
        await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id3}", new { });

        var quarto = await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id4}", new { });
        Assert.True((await quarto.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var remover = await cliente.PostAsJsonAsync($"/api/sugestoes/removervoto/{id1}", new { });
        Assert.False((await remover.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var realocado = await cliente.PostAsJsonAsync($"/api/sugestoes/votar/{id4}", new { });
        Assert.False((await realocado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Admin_NaoConsegueVotar()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "S1");

        var response = await admin.PostAsJsonAsync($"/api/sugestoes/votar/{id}", new { });

        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }
}
