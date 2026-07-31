using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Domain.Entities;

public class Sugestao
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ResultadoEsperado { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int AutorId { get; set; }
    public Usuario? Autor { get; set; }
    public StatusSugestao Status { get; set; } = StatusSugestao.EmModeracao;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataModeracao { get; set; }
    public string? MotivoRejeicao { get; set; }
    public int? ModeradorId { get; set; }
    public Usuario? Moderador { get; set; }

    public ICollection<Voto> Votos { get; set; } = new List<Voto>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
