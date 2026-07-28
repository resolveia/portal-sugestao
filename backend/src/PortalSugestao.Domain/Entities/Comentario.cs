namespace PortalSugestao.Domain.Entities;

public class Comentario
{
    public int Id { get; set; }
    public int SugestaoId { get; set; }
    public Sugestao? Sugestao { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
