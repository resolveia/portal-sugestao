namespace PortalSugestao.Infrastructure.Notificacoes;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1025;
    public string From { get; set; } = "portal-sugestao@local.test";
}
