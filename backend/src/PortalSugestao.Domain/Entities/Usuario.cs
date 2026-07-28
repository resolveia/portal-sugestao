using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
    public string ErpUserId { get; set; } = string.Empty;
    public RoleUsuario Role { get; set; } = RoleUsuario.Cliente;

    public ICollection<Sugestao> Sugestoes { get; set; } = new List<Sugestao>();
    public ICollection<Voto> Votos { get; set; } = new List<Voto>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
