namespace PortalSugestao.Domain.Entities;

public class Voto
{
    public int Id { get; set; }
    public int SugestaoId { get; set; }
    public Sugestao? Sugestao { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime DataVoto { get; set; } = DateTime.UtcNow;
}
