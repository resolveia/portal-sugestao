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

    private readonly PortalSugestaoDbContext _db;
    private readonly NotificacaoService _notificacaoService;

    public SugestoesController(PortalSugestaoDbContext db, NotificacaoService notificacaoService)
    {
        _db = db;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Lista sugestões publicadas, ordenadas por número de votos (ranking — seção 5.4 do PRD).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SugestaoDto>>> Listar()
    {
        var currentUserId = CurrentUserId();

        var sugestoes = await _db.Sugestoes
            .Where(s => s.Status == StatusSugestao.Publicada)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .OrderByDescending(s => s.Votos.Count)
            .ToListAsync();

        return Ok(sugestoes.Select(s => ToDto(s, currentUserId)));
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
            .Where(s => s.Status == StatusSugestao.EmModeracao)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .OrderBy(s => s.DataCriacao)
            .ToListAsync();

        return Ok(sugestoes.Select(s => ToDto(s, currentUserId)));
    }

    /// <summary>
    /// Cadastra uma nova sugestão, sempre entrando com status "Em moderação" (regra 7.1 do PRD).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SugestaoDto>> Criar(CreateSugestaoRequest request)
    {
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
            CategoriaId = request.CategoriaId,
            AutorId = autor.Id,
            Status = StatusSugestao.EmModeracao
        };

        _db.Sugestoes.Add(sugestao);
        await _db.SaveChangesAsync();

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
        var currentUserId = CurrentUserId();

        var sugestao = await _db.Sugestoes
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

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return BadRequest("Categoria inválida.");
        }

        sugestao.Titulo = request.Titulo;
        sugestao.Descricao = request.Descricao;
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

    private static SugestaoDto ToDto(Sugestao s, int currentUserId) => new(
        s.Id,
        s.Titulo,
        s.Descricao,
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
