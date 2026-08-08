using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Infrastructure.Notificacoes;

/// <summary>
/// Envia e-mails de notificação (regra 7.5 do PRD) e registra o envio em NotificacaoLog.
/// Em dev, aponta para um SMTP local de teste (MailHog) — troque o host/porta em produção.
/// </summary>
public class NotificacaoService
{
    private readonly PortalSugestaoDbContext _db;
    private readonly SmtpOptions _options;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(PortalSugestaoDbContext db, IOptions<SmtpOptions> options, ILogger<NotificacaoService> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task NotificarAsync(Usuario destinatario, TipoNotificacao tipo, Sugestao sugestao)
    {
        var (assunto, corpo) = MontarConteudo(tipo, sugestao);

        try
        {
            await EnviarEmailAsync(destinatario.Email, assunto, corpo);
        }
        catch (Exception ex)
        {
            // Falha no envio não deve derrubar a ação principal (aprovar/rejeitar/comentar).
            _logger.LogWarning(ex, "Falha ao enviar e-mail de notificação {Tipo} para {Email}", tipo, destinatario.Email);
        }

        _db.NotificacaoLogs.Add(new NotificacaoLog
        {
            UsuarioId = destinatario.Id,
            Tipo = tipo,
            SugestaoId = sugestao.Id,
            DataEnvio = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private static (string Assunto, string Corpo) MontarConteudo(TipoNotificacao tipo, Sugestao sugestao) => tipo switch
    {
        TipoNotificacao.SugestaoAprovada => (
            $"Sua sugestão \"{sugestao.Titulo}\" foi aprovada",
            $"Boas notícias! Sua sugestão \"{sugestao.Titulo}\" foi aprovada e já está publicada no Portal de Sugestões."),
        TipoNotificacao.SugestaoRejeitada => (
            $"Sua sugestão \"{sugestao.Titulo}\" foi rejeitada",
            $"Sua sugestão \"{sugestao.Titulo}\" foi rejeitada.\nMotivo: {sugestao.MotivoRejeicao}"),
        TipoNotificacao.NovoComentario => (
            $"Nova resposta em \"{sugestao.Titulo}\"",
            $"A equipe respondeu à sua sugestão \"{sugestao.Titulo}\". Acesse o Portal de Sugestões para ver o comentário."),
        TipoNotificacao.SugestaoLancada => (
            $"Sua sugestão \"{sugestao.Titulo}\" foi lançada!",
            $"Boas notícias! Sua sugestão \"{sugestao.Titulo}\" acabou de ser lançada. Obrigado por contribuir com o Portal de Sugestões."),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private async Task EnviarEmailAsync(string destinatarioEmail, string assunto, string corpo)
    {
        var mensagem = new MimeMessage();
        mensagem.From.Add(MailboxAddress.Parse(_options.From));
        mensagem.To.Add(MailboxAddress.Parse(destinatarioEmail));
        mensagem.Subject = assunto;
        mensagem.Body = new TextPart("plain") { Text = corpo };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.None);
        await client.SendAsync(mensagem);
        await client.DisconnectAsync(true);
    }
}
