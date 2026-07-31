using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalSugestao.Tests;

public class SugestoesControllerTests
{
    [Fact]
    public async Task Criar_EntraComoEmModeracao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug1@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes",
            new { produtoId = 1, titulo = "Nova", descricao = "Desc", resultadoEsperado = "Resultado", categoriaId });
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("EmModeracao", dto.GetProperty("status").GetString());
        Assert.Equal("AJORS.OOH", dto.GetProperty("produtoNome").GetString());
    }

    [Theory]
    [InlineData("", "Desc", "Resultado")]
    [InlineData("Titulo", "", "Resultado")]
    [InlineData("Titulo", "Desc", "")]
    public async Task Criar_CamposObrigatoriosVaziosDaBadRequest(string titulo, string descricao, string resultadoEsperado)
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug4@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes",
            new { produtoId = 1, titulo, descricao, resultadoEsperado, categoriaId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_ProdutoInvalidoDaBadRequest()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.sug5@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.sug5@empresa.com", "Cliente", "Empresa", "Cliente");

        var response = await cliente.PostAsJsonAsync(
            "/api/sugestoes",
            new { produtoId = 9999, titulo = "T", descricao = "D", resultadoEsperado = "R", categoriaId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        await admin.PutAsync($"/api/sugestoes/{idPublicada}/aprovar", null);

        var response = await cliente.GetAsync("/api/sugestoes");
        var lista = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = lista.EnumerateArray().Select(s => s.GetProperty("id").GetInt32()).ToList();

        Assert.Contains(idPublicada, ids);
        Assert.DoesNotContain(idNaoPublicada, ids);
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

        var respostaOutro = await outroCliente.PutAsJsonAsync(
            $"/api/sugestoes/{id}",
            new { produtoId = 1, titulo = "Hack", descricao = "Hack", resultadoEsperado = "Hack", categoriaId });
        Assert.Equal(HttpStatusCode.Forbidden, respostaOutro.StatusCode);

        var respostaAutor = await autor.PutAsJsonAsync(
            $"/api/sugestoes/{id}",
            new { produtoId = 1, titulo = "Editado", descricao = "Editado", resultadoEsperado = "Editado", categoriaId });
        Assert.Equal(HttpStatusCode.OK, respostaAutor.StatusCode);

        await admin.PutAsync($"/api/sugestoes/{id}/aprovar", null);

        var respostaPosModeracao = await autor.PutAsJsonAsync(
            $"/api/sugestoes/{id}",
            new { produtoId = 1, titulo = "Tarde demais", descricao = "x", resultadoEsperado = "x", categoriaId });
        Assert.Equal(HttpStatusCode.Conflict, respostaPosModeracao.StatusCode);
    }
}
