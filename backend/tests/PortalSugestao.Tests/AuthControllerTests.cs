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
}
