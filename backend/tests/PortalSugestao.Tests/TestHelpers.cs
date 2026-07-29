using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

internal static class TestHelpers
{
    /// <summary>Loga via /api/auth/mock-login e devolve um HttpClient já com o Bearer configurado.</summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this CustomWebApplicationFactory factory, string email, string nome, string empresa, string role)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/mock-login", new { email, nome, empresa, role });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<int> CriarCategoriaAsync(this HttpClient adminClient, string nome = "Financeiro")
    {
        var response = await adminClient.PostAsJsonAsync("/api/categorias", new { nome });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    public static async Task<int> CriarSugestaoAsync(
        this HttpClient client, int categoriaId, string titulo = "Sugestão de teste", string descricao = "Descrição de teste")
    {
        var response = await client.PostAsJsonAsync("/api/sugestoes", new { titulo, descricao, categoriaId });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }
}
