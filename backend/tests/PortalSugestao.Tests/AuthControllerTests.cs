using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task MockLogin_CriaUsuarioEDevolveTokenValido()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/mock-login", new
        {
            email = "auth.teste@empresa.com",
            nome = "Auth Teste",
            empresa = "Empresa Exemplo",
            role = "Cliente"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
        Assert.Equal("auth.teste@empresa.com", body.GetProperty("email").GetString());
        Assert.Equal("Cliente", body.GetProperty("role").GetString());
    }

    [Fact]
    public async Task MockLogin_MesmoEmailNaoDuplicaUsuario()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new { email = "duplicado@empresa.com", nome = "Nome", empresa = "Empresa", role = "Cliente" };

        var r1 = await client.PostAsJsonAsync("/api/auth/mock-login", request);
        var r2 = await client.PostAsJsonAsync("/api/auth/mock-login", request);

        var id1 = (await r1.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("usuarioId").GetInt32();
        var id2 = (await r2.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("usuarioId").GetInt32();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public async Task MockLogin_DefineCookieHttpOnlyDeSessao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/mock-login", new
        {
            email = "cookie.teste@empresa.com",
            nome = "Cookie Teste",
            empresa = "Empresa Exemplo",
            role = "Cliente"
        });

        response.EnsureSuccessStatusCode();
        var setCookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("portal_sugestao_session=", setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TokensDemo_RetornaTokensParaAdminEClienteQueLogamComSucesso()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var demo = await (await client.GetAsync("/api/auth/tokens-demo")).Content.ReadFromJsonAsync<JsonElement>();
        var tokenAdmin = demo.GetProperty("admin").GetString();
        var tokenCliente = demo.GetProperty("cliente").GetString();

        var respAdmin = await client.PostAsJsonAsync("/api/auth/login-token", new { token = tokenAdmin });
        var bodyAdmin = await respAdmin.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(bodyAdmin.GetProperty("erro").GetBoolean());
        Assert.Equal("AdminInterno", bodyAdmin.GetProperty("usuario").GetProperty("role").GetString());

        var respCliente = await client.PostAsJsonAsync("/api/auth/login-token", new { token = tokenCliente });
        var bodyCliente = await respCliente.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(bodyCliente.GetProperty("erro").GetBoolean());
        Assert.Equal("Cliente", bodyCliente.GetProperty("usuario").GetProperty("role").GetString());
    }

    [Fact]
    public async Task LoginToken_ComTokenInvalido_Retorna200ComErroTrue()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login-token", new { token = "isso-nao-e-um-token-valido" });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("erro").GetBoolean());
    }
}
