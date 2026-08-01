using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class VotosControllerTests
{
    private static async Task<int> CriarEPublicarAsync(HttpClient admin, HttpClient autor, int categoriaId, string titulo)
    {
        var id = await autor.CriarSugestaoAsync(categoriaId, titulo);
        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);
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

        var response = await cliente.PostAsync($"/api/sugestoes/{id}/votos", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ranking = await (await cliente.GetAsync("/api/sugestoes")).Content.ReadFromJsonAsync<JsonElement>();
        var item = ranking.GetProperty("items").EnumerateArray().First(s => s.GetProperty("id").GetInt32() == id);

        Assert.Equal(1, item.GetProperty("totalVotos").GetInt32());
        Assert.True(item.GetProperty("votadoPorMim").GetBoolean());
        Assert.Equal(1, ranking.GetProperty("votosUsadosPeloUsuarioAtual").GetInt32());
    }

    [Fact]
    public async Task Votar_DuasVezesNaMesmaSugestaoDaConflito()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "Sugestao 1");

        await cliente.PostAsync($"/api/sugestoes/{id}/votos", null);
        var segundo = await cliente.PostAsync($"/api/sugestoes/{id}/votos", null);

        Assert.Equal(HttpStatusCode.Conflict, segundo.StatusCode);
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

        await cliente.PostAsync($"/api/sugestoes/{id1}/votos", null);
        await cliente.PostAsync($"/api/sugestoes/{id2}/votos", null);
        await cliente.PostAsync($"/api/sugestoes/{id3}/votos", null);

        var quarto = await cliente.PostAsync($"/api/sugestoes/{id4}/votos", null);
        Assert.Equal(HttpStatusCode.Conflict, quarto.StatusCode);

        var remover = await cliente.DeleteAsync($"/api/sugestoes/{id1}/votos");
        Assert.Equal(HttpStatusCode.OK, remover.StatusCode);

        var realocado = await cliente.PostAsync($"/api/sugestoes/{id4}/votos", null);
        Assert.Equal(HttpStatusCode.OK, realocado.StatusCode);
    }

    [Fact]
    public async Task Admin_NaoConsegueVotar()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.vot4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.vot4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await CriarEPublicarAsync(admin, cliente, categoriaId, "S1");

        var response = await admin.PostAsync($"/api/sugestoes/{id}/votos", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
