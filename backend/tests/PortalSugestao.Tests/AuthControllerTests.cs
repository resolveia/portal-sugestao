using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Sessao_CriaUsuarioEDefineCookieDeSessao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sessao", new
        {
            Nome = "Auth Teste",
            Login = "auth.teste",
            Id = 123,
            EmpresaId = "EMP1",
            AdminPortalSugestoes = false
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("Erro").GetBoolean());
        Assert.Equal("Auth Teste", body.GetProperty("Usuario").GetProperty("Nome").GetString());
        Assert.Equal("Cliente", body.GetProperty("Usuario").GetProperty("Role").GetString());

        var setCookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("portal_sugestao_session=", setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sessao_MesmaEmpresaEIdNaoDuplicaUsuario()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new { Nome = "Nome", Login = "login.dup", Id = 999, EmpresaId = "EMP2", AdminPortalSugestoes = false };

        var r1 = await client.PostAsJsonAsync("/api/auth/sessao", request);
        var r2 = await client.PostAsJsonAsync("/api/auth/sessao", request);

        var id1 = (await r1.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Usuario").GetProperty("Id").GetInt32();
        var id2 = (await r2.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Usuario").GetProperty("Id").GetInt32();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public async Task Sessao_AdminPortalSugestoesTrueViraRoleAdminInterno()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sessao", new
        {
            Nome = "Admin Teste",
            Login = "admin.teste",
            Id = 456,
            EmpresaId = "EMP1",
            AdminPortalSugestoes = true
        });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("AdminInterno", body.GetProperty("Usuario").GetProperty("Role").GetString());
    }

    [Fact]
    public async Task Logout_LimpaOCookieDeSessao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/logout", new { });

        response.EnsureSuccessStatusCode();
        var setCookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("portal_sugestao_session=", setCookie);
        Assert.Contains("expires=Thu, 01 Jan 1970", setCookie, StringComparison.OrdinalIgnoreCase);
    }
}
