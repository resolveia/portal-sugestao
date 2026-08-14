using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

internal static class TestHelpers
{
    /// <summary>
    /// Estabelece a sessão local via /api/auth/sessao (equivalente a já ter logado contra a
    /// api_authentication real — nesta aplicação local o login é sempre considerado válido) e
    /// devolve um HttpClient já com o Bearer configurado. O Id é derivado do e-mail pra ficar
    /// estável entre chamadas com os mesmos dados de teste, sem colidir entre usuários diferentes.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this CustomWebApplicationFactory factory, string email, string nome, string empresa, string role)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sessao", new
        {
            Nome = nome,
            Login = email,
            Id = Math.Abs(email.GetHashCode()),
            EmpresaId = empresa,
            AdminPortalSugestoes = role == "AdminInterno"
        });
        response.EnsureSuccessStatusCode();

        var token = ExtrairTokenDoCookie(response);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string ExtrairTokenDoCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.GetValues("Set-Cookie").Single();
        var valor = setCookie.Split(';')[0].Split('=', 2)[1];
        return Uri.UnescapeDataString(valor);
    }

    public static async Task<int> CriarCategoriaAsync(this HttpClient adminClient, string nome = "Financeiro")
    {
        var response = await adminClient.PostAsJsonAsync("/api/categorias/salvar", new { nome });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("Categoria").GetProperty("Id").GetInt32();
    }

    public static async Task<int> CriarProdutoAsync(this HttpClient adminClient, string nome = "Produto Teste")
    {
        var response = await adminClient.PostAsJsonAsync("/api/produtos/salvar", new { nome });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("Produto").GetProperty("Id").GetInt32();
    }

    /// <summary>produtoId=1 por padrão: aponta pro "AJORS.OOH" semeado via HasData, sempre presente.</summary>
    public static async Task<int> CriarSugestaoAsync(
        this HttpClient client,
        int categoriaId,
        string titulo = "Sugestão de teste",
        string descricao = "Descrição de teste",
        string resultadoEsperado = "Resultado esperado de teste",
        int produtoId = 1)
    {
        var response = await client.PostAsJsonAsync(
            "/api/sugestoes/salvar",
            new { produtoId, titulo, descricao, resultadoEsperado, categoriaId });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("Sugestao").GetProperty("Id").GetInt32();
    }
}
