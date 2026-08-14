using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class SugestoesControllerTests
{
    [Fact]
    public async Task Salvar_EntraComoEmModeracao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes/salvar",
            new { produtoId = 1, titulo = "Nova", descricao = "Desc", resultadoEsperado = "Resultado", categoriaId });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sugestao = body.GetProperty("Sugestao");

        Assert.False(body.GetProperty("Erro").GetBoolean());
        Assert.Equal("EmModeracao", sugestao.GetProperty("Status").GetString());
        Assert.Equal("AJORS.OOH", sugestao.GetProperty("ProdutoNome").GetString());
    }

    [Theory]
    [InlineData("", "Desc", "Resultado")]
    [InlineData("Titulo", "", "Resultado")]
    [InlineData("Titulo", "Desc", "")]
    public async Task Salvar_CamposObrigatoriosVaziosDevolveErro(string titulo, string descricao, string resultadoEsperado)
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug4@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes/salvar",
            new { produtoId = 1, titulo, descricao, resultadoEsperado, categoriaId });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Salvar_ProdutoInvalidoDevolveErro()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug5@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes/salvar",
            new { produtoId = 9999, titulo = "T", descricao = "D", resultadoEsperado = "R", categoriaId });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("Erro").GetBoolean());
    }

    [Fact]
    public async Task Listar_MostraApenasPublicadas()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug2@empresa.com", "Cliente", "Empresa", "Cliente");

        var idNaoPublicada = await cliente.CriarSugestaoAsync(categoriaId, "Nao publicada");
        var idPublicada = await cliente.CriarSugestaoAsync(categoriaId, "Publicada");
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{idPublicada}", new { });

        var response = await cliente.PostAsJsonAsync("/api/sugestoes/listar", new { Skip = 0, Take = 20 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = body.GetProperty("Sugestoes").EnumerateArray().Select(s => s.GetProperty("Id").GetInt32()).ToList();

        Assert.Contains(idPublicada, ids);
        Assert.DoesNotContain(idNaoPublicada, ids);
    }

    [Fact]
    public async Task Listar_RespeitaSkipETakeERetornaTotal()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug6@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug6@empresa.com", "Cliente", "Empresa", "Cliente");

        for (var i = 0; i < 5; i++)
        {
            var id = await cliente.CriarSugestaoAsync(categoriaId, $"Paginada {i}");
            await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });
        }

        var pagina1 = await (await cliente.PostAsJsonAsync("/api/sugestoes/listar", new { Skip = 0, Take = 2 })).Content.ReadFromJsonAsync<JsonElement>();
        var pagina2 = await (await cliente.PostAsJsonAsync("/api/sugestoes/listar", new { Skip = 2, Take = 2 })).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(2, pagina1.GetProperty("Sugestoes").GetArrayLength());
        Assert.Equal(2, pagina2.GetProperty("Sugestoes").GetArrayLength());
        Assert.True(pagina1.GetProperty("Total").GetInt32() >= 5);

        var idsPagina1 = pagina1.GetProperty("Sugestoes").EnumerateArray().Select(s => s.GetProperty("Id").GetInt32()).ToList();
        var idsPagina2 = pagina2.GetProperty("Sugestoes").EnumerateArray().Select(s => s.GetProperty("Id").GetInt32()).ToList();
        Assert.Empty(idsPagina1.Intersect(idsPagina2));
    }

    [Fact]
    public async Task Editar_SoAutorEnquantoEmModeracao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var autor = await factory.CreateAuthenticatedClientAsync("autor.sug3@empresa.com", "Autor", "Empresa", "Cliente");
        var outroCliente = await factory.CreateAuthenticatedClientAsync("outro.sug3@empresa.com", "Outro", "Empresa", "Cliente");

        var id = await autor.CriarSugestaoAsync(categoriaId, "Original");

        var respostaOutro = await outroCliente.PostAsJsonAsync(
            $"/api/sugestoes/editar/{id}",
            new { produtoId = 1, titulo = "Hack", descricao = "Hack", resultadoEsperado = "Hack", categoriaId });
        Assert.True((await respostaOutro.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        var respostaAutor = await autor.PostAsJsonAsync(
            $"/api/sugestoes/editar/{id}",
            new { produtoId = 1, titulo = "Editado", descricao = "Editado", resultadoEsperado = "Editado", categoriaId });
        Assert.False((await respostaAutor.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());

        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        var respostaPosModeracao = await autor.PostAsJsonAsync(
            $"/api/sugestoes/editar/{id}",
            new { produtoId = 1, titulo = "Tarde demais", descricao = "x", resultadoEsperado = "x", categoriaId });
        Assert.True((await respostaPosModeracao.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("Erro").GetBoolean());
    }
}
