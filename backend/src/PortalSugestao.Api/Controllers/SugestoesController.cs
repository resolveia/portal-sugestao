using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;
using PortalSugestao.Infrastructure.Notificacoes;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/sugestoes")]
[Authorize]
public class SugestoesController : ControllerBase
{
    private const int LimiteVotosPorUsuario = 3;
    private const int TamanhoPaginaMaximo = 100;

    private readonly PortalSugestaoDbContext _db;
    private readonly NotificacaoService _notificacaoService;

    public SugestoesController(PortalSugestaoDbContext db, NotificacaoService notificacaoService)
    {
        _db = db;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Lista sugestões publicadas, ordenadas por número de votos (ranking — seção 5.4 do PRD).
    /// Paginado no servidor (RNF seção 11 — ver docs/performance-report.md: sem paginação, o payload
    /// cresce com o total de sugestões publicadas e vira o próximo gargalo depois da otimização de
    /// query). VotosUsadosPeloUsuarioAtual vem à parte do total de votos ativos do usuário (regra 7.2),
    /// já que o limite de 3 é sobre o total, não sobre a página atual.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SugestoesPaginadasDto>> Listar(int skip = 0, int take = 20)
    {
        var currentUserId = CurrentUserId();
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, TamanhoPaginaMaximo);

        var query = _db.Sugestoes
            .AsNoTracking()
            .Where(s => s.Status == StatusSugestao.Publicada);

        var total = await query.CountAsync();

        var sugestoes = await query
            .OrderByDescending(s => s.Votos.Count)
            .Skip(skip)
            .Take(take)
            .Select(s => new SugestaoDto(
                s.Id,
                s.Titulo,
                s.Descricao,
                s.ResultadoEsperado,
                s.ProdutoId,
                s.Produto!.Nome,
                s.CategoriaId,
                s.Categoria!.Nome,
                s.AutorId,
                s.Autor!.Nome,
                s.Status,
                s.DataCriacao,
                s.Votos.Count,
                s.Votos.Any(v => v.UsuarioId == currentUserId),
                s.DataModeracao,
                s.MotivoRejeicao,
                s.Moderador == null ? null : s.Moderador.Nome))
            .ToListAsync();

        var votosUsados = await _db.Votos.CountAsync(v => v.UsuarioId == currentUserId);

        return Ok(new SugestoesPaginadasDto(sugestoes, total, votosUsados));
    }

    /// <summary>
    /// Fila de moderação: sugestões ainda não aprovadas/rejeitadas (regra 7.1 do PRD).
    /// </summary>
    [HttpGet("pendentes")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<IEnumerable<SugestaoDto>>> Pendentes()
    {
        var currentUserId = CurrentUserId();

        var sugestoes = await _db.Sugestoes
            .AsNoTracking()
            .Where(s => s.Status == StatusSugestao.EmModeracao)
            .OrderBy(s => s.DataCriacao)
            .Select(s => new SugestaoDto(
                s.Id,
                s.Titulo,
                s.Descricao,
                s.ResultadoEsperado,
                s.ProdutoId,
                s.Produto!.Nome,
                s.CategoriaId,
                s.Categoria!.Nome,
                s.AutorId,
                s.Autor!.Nome,
                s.Status,
                s.DataCriacao,
                s.Votos.Count,
                s.Votos.Any(v => v.UsuarioId == currentUserId),
                s.DataModeracao,
                s.MotivoRejeicao,
                s.Moderador == null ? null : s.Moderador.Nome))
            .ToListAsync();

        return Ok(sugestoes);
    }

    /// <summary>
    /// Cadastra uma nova sugestão, sempre entrando com status "Em moderação" (regra 7.1 do PRD).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SugestaoDto>> Criar(CreateSugestaoRequest request)
    {
        if (!CamposObrigatoriosValidos(request.Titulo, request.Descricao, request.ResultadoEsperado))
        {
            return BadRequest("Título, descrição e resultado esperado são obrigatórios.");
        }

        var produto = await _db.Produtos.FindAsync(request.ProdutoId);
        if (produto is null)
        {
            return BadRequest("Produto inválido.");
        }

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return BadRequest("Categoria inválida.");
        }

        var autor = await _db.Usuarios.FindAsync(CurrentUserId());
        if (autor is null)
        {
            return Unauthorized();
        }

        var sugestao = new Sugestao
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            ResultadoEsperado = request.ResultadoEsperado,
            ProdutoId = request.ProdutoId,
            CategoriaId = request.CategoriaId,
            AutorId = autor.Id,
            Status = StatusSugestao.EmModeracao
        };

        _db.Sugestoes.Add(sugestao);
        await _db.SaveChangesAsync();

        sugestao.Produto = produto;
        sugestao.Categoria = categoria;
        sugestao.Autor = autor;

        return CreatedAtAction(nameof(Listar), ToDto(sugestao, autor.Id));
    }

    /// <summary>
    /// Edição pela própria pessoa autora, permitida apenas enquanto a sugestão ainda está em moderação.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SugestaoDto>> Editar(int id, CreateSugestaoRequest request)
    {
        if (!CamposObrigatoriosValidos(request.Titulo, request.Descricao, request.ResultadoEsperado))
        {
            return BadRequest("Título, descrição e resultado esperado são obrigatórios.");
        }

        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.AutorId != currentUserId)
        {
            return Forbid();
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Conflict("Sugestão já foi moderada e não pode mais ser editada.");
        }

        var produto = await _db.Produtos.FindAsync(request.ProdutoId);
        if (produto is null)
        {
            return BadRequest("Produto inválido.");
        }

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return BadRequest("Categoria inválida.");
        }

        sugestao.Titulo = request.Titulo;
        sugestao.Descricao = request.Descricao;
        sugestao.ResultadoEsperado = request.ResultadoEsperado;
        sugestao.ProdutoId = request.ProdutoId;
        sugestao.Produto = produto;
        sugestao.CategoriaId = request.CategoriaId;
        sugestao.Categoria = categoria;

        await _db.SaveChangesAsync();

        return Ok(ToDto(sugestao, currentUserId));
    }

    /// <summary>
    /// Aprova uma sugestão em moderação, publicando-a (regra 7.1 do PRD).
    /// </summary>
    [HttpPut("{id}/aprovar")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<SugestaoDto>> Aprovar(int id)
    {
        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Conflict("Sugestão já foi moderada.");
        }

        sugestao.Status = StatusSugestao.Publicada;
        sugestao.DataModeracao = DateTime.UtcNow;
        sugestao.ModeradorId = currentUserId;

        await _db.SaveChangesAsync();

        sugestao.Moderador = await _db.Usuarios.FindAsync(sugestao.ModeradorId);
        await _notificacaoService.NotificarAsync(sugestao.Autor!, TipoNotificacao.SugestaoAprovada, sugestao);

        return Ok(ToDto(sugestao, currentUserId));
    }

    /// <summary>
    /// Rejeita uma sugestão em moderação, com justificativa (regra 7.1 do PRD).
    /// </summary>
    [HttpPut("{id}/rejeitar")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<SugestaoDto>> Rejeitar(int id, RejeitarSugestaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            return BadRequest("Motivo é obrigatório.");
        }

        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Conflict("Sugestão já foi moderada.");
        }

        sugestao.Status = StatusSugestao.Rejeitada;
        sugestao.DataModeracao = DateTime.UtcNow;
        sugestao.MotivoRejeicao = request.Motivo;
        sugestao.ModeradorId = currentUserId;

        await _db.SaveChangesAsync();

        sugestao.Moderador = await _db.Usuarios.FindAsync(sugestao.ModeradorId);
        await _notificacaoService.NotificarAsync(sugestao.Autor!, TipoNotificacao.SugestaoRejeitada, sugestao);

        return Ok(ToDto(sugestao, currentUserId));
    }

    /// <summary>
    /// Vota em uma sugestão publicada. Limite de 3 votos ativos por cliente, um voto por sugestão (regra 7.2 do PRD).
    /// </summary>
    [HttpPost("{id}/votos")]
    [Authorize(Roles = nameof(RoleUsuario.Cliente))]
    public async Task<ActionResult<SugestaoDto>> Votar(int id)
    {
        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Conflict("Só é possível votar em sugestões publicadas.");
        }

        if (sugestao.Votos.Any(v => v.UsuarioId == currentUserId))
        {
            return Conflict("Você já votou nesta sugestão.");
        }

        var votosAtivos = await _db.Votos.CountAsync(v => v.UsuarioId == currentUserId);
        if (votosAtivos >= LimiteVotosPorUsuario)
        {
            return Conflict($"Limite de {LimiteVotosPorUsuario} votos ativos atingido. Remova um voto para votar em outra sugestão.");
        }

        sugestao.Votos.Add(new Voto { SugestaoId = sugestao.Id, UsuarioId = currentUserId });
        await _db.SaveChangesAsync();

        return Ok(ToDto(sugestao, currentUserId));
    }

    /// <summary>
    /// Remove o voto do usuário autenticado nesta sugestão — usado para realocar o voto para outra (regra 7.2 do PRD).
    /// </summary>
    [HttpDelete("{id}/votos")]
    [Authorize(Roles = nameof(RoleUsuario.Cliente))]
    public async Task<ActionResult<SugestaoDto>> RemoverVoto(int id)
    {
        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sugestao is null)
        {
            return NotFound();
        }

        var voto = sugestao.Votos.FirstOrDefault(v => v.UsuarioId == currentUserId);
        if (voto is null)
        {
            return NotFound("Você não tem voto nesta sugestão.");
        }

        sugestao.Votos.Remove(voto);
        _db.Votos.Remove(voto);
        await _db.SaveChangesAsync();

        return Ok(ToDto(sugestao, currentUserId));
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool CamposObrigatoriosValidos(string titulo, string descricao, string resultadoEsperado) =>
        !string.IsNullOrWhiteSpace(titulo) && !string.IsNullOrWhiteSpace(descricao) && !string.IsNullOrWhiteSpace(resultadoEsperado);

    private static SugestaoDto ToDto(Sugestao s, int currentUserId) => new(
        s.Id,
        s.Titulo,
        s.Descricao,
        s.ResultadoEsperado,
        s.ProdutoId,
        s.Produto!.Nome,
        s.CategoriaId,
        s.Categoria!.Nome,
        s.AutorId,
        s.Autor!.Nome,
        s.Status,
        s.DataCriacao,
        s.Votos.Count,
        s.Votos.Any(v => v.UsuarioId == currentUserId),
        s.DataModeracao,
        s.MotivoRejeicao,
        s.Moderador?.Nome);
}
