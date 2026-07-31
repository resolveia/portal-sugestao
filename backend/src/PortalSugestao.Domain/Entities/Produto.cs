namespace PortalSugestao.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public ICollection<Sugestao> Sugestoes { get; set; } = new List<Sugestao>();
}
