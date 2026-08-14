using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Tests;

public class NotificacoesTests
{
    [Fact]
    public async Task Aprovar_GravaNotificacaoLog()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.notif1@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.notif1@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        await using var db = factory.CreateDbContext();
        var existe = await db.NotificacaoLogs.AnyAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.SugestaoAprovada);
        Assert.True(existe);
    }

    [Fact]
    public async Task Rejeitar_GravaNotificacaoLog()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.notif2@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.notif2@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);

        await admin.PostAsJsonAsync($"/api/sugestoes/rejeitar/{id}", new { motivo = "Duplicada" });

        await using var db = factory.CreateDbContext();
        var existe = await db.NotificacaoLogs.AnyAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.SugestaoRejeitada);
        Assert.True(existe);
    }

    [Fact]
    public async Task ComentarioDoAdmin_GravaLog_ComentarioDoClienteNao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.notif3@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.notif3@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        await cliente.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Comentario cliente" });

        await using (var db = factory.CreateDbContext())
        {
            var count = await db.NotificacaoLogs.CountAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.NovoComentario);
            Assert.Equal(0, count);
        }

        await admin.PostAsJsonAsync($"/api/sugestoes/{id}/comentarios/salvar", new { texto = "Comentario admin" });

        await using (var db = factory.CreateDbContext())
        {
            var count = await db.NotificacaoLogs.CountAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.NovoComentario);
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task MarcarComoLancada_GravaNotificacaoLog_SoNaTransicao()
    {
        await using var factory = new CustomWebApplicationFactory();
        var admin = await factory.CreateAuthenticatedClientAsync("admin.notif4@empresa.com", "Admin", "Empresa", "AdminInterno");
        var categoriaId = await admin.CriarCategoriaAsync();
        var cliente = await factory.CreateAuthenticatedClientAsync("cliente.notif4@empresa.com", "Cliente", "Empresa", "Cliente");
        var id = await cliente.CriarSugestaoAsync(categoriaId);
        await admin.PostAsJsonAsync($"/api/sugestoes/aprovar/{id}", new { });

        await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Planejado" });

        await using (var db = factory.CreateDbContext())
        {
            var count = await db.NotificacaoLogs.CountAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.SugestaoLancada);
            Assert.Equal(0, count);
        }

        await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Lancado" });
        await admin.PostAsJsonAsync($"/api/sugestoes/roadmap/{id}", new { estagio = "Lancado" });

        await using (var db = factory.CreateDbContext())
        {
            var count = await db.NotificacaoLogs.CountAsync(l => l.SugestaoId == id && l.Tipo == TipoNotificacao.SugestaoLancada);
            Assert.Equal(1, count);
        }
    }
}
