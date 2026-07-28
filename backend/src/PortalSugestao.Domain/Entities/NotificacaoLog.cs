using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Domain.Entities;

public class NotificacaoLog
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoNotificacao Tipo { get; set; }
    public int SugestaoId { get; set; }
    public Sugestao? Sugestao { get; set; }
    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
}
