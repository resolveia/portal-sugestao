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
    /// Paginado no servidor (RNF seção 11 — ver docs/performance-report.md).
    /// </summary>
    [HttpPost("listar")]
    public async Task<ActionResult<ListarSugestoesResponse>> Listar(ListarSugestoesRequest request)
    {
        var currentUserId = CurrentUserId();
        var skip = Math.Max(0, request.Skip);
        var take = Math.Clamp(request.Take <= 0 ? 20 : request.Take, 1, TamanhoPaginaMaximo);

        var query = _db.Sugestoes
            .AsNoTracking()
            .Where(s => s.Status == StatusSugestao.Publicada);

        var total = await query.CountAsync();

        var sugestoes = await query
            .OrderByDescending(s => s.Votos.Count)
            .Skip(skip)
            .Take(take)
            .Select(s => new SugestaoDto(
                s.Id, s.Titulo, s.Descricao, s.ResultadoEsperado, s.ProdutoId, s.Produto!.Nome,
                s.CategoriaId, s.Categoria!.Nome, s.AutorId, s.Autor!.Nome, s.Status, s.EstagioRoadmap,
                s.DataCriacao, s.Votos.Count, s.Votos.Any(v => v.UsuarioId == currentUserId),
                s.DataModeracao, s.MotivoRejeicao, s.Moderador == null ? null : s.Moderador.Nome))
            .ToListAsync();

        var votosUsados = await _db.Votos.CountAsync(v => v.UsuarioId == currentUserId);

        return Ok(new ListarSugestoesResponse(false, null, sugestoes, total, votosUsados));
    }

    /// <summary>Fila de moderação: sugestões ainda não aprovadas/rejeitadas (regra 7.1 do PRD).</summary>
    [HttpPost("pendentes")]
    public async Task<ActionResult<PendentesResponse>> Pendentes()
    {
        if (!IsAdmin())
        {
            return Ok(new PendentesResponse(true, "Operação não permitida.", null));
        }

        var currentUserId = CurrentUserId();

        var sugestoes = await _db.Sugestoes
            .AsNoTracking()
            .Where(s => s.Status == StatusSugestao.EmModeracao)
            .OrderBy(s => s.DataCriacao)
            .Select(s => new SugestaoDto(
                s.Id, s.Titulo, s.Descricao, s.ResultadoEsperado, s.ProdutoId, s.Produto!.Nome,
                s.CategoriaId, s.Categoria!.Nome, s.AutorId, s.Autor!.Nome, s.Status, s.EstagioRoadmap,
                s.DataCriacao, s.Votos.Count, s.Votos.Any(v => v.UsuarioId == currentUserId),
                s.DataModeracao, s.MotivoRejeicao, s.Moderador == null ? null : s.Moderador.Nome))
            .ToListAsync();

        return Ok(new PendentesResponse(false, null, sugestoes));
    }

    /// <summary>Cadastra uma nova sugestão, sempre entrando com status "Em moderação" (regra 7.1 do PRD).</summary>
    [HttpPost("salvar")]
    public async Task<ActionResult<SugestaoResponse>> Salvar(CreateSugestaoRequest request)
    {
        if (!CamposObrigatoriosValidos(request.Titulo, request.Descricao, request.ResultadoEsperado))
        {
            return Ok(new SugestaoResponse(true, "Título, descrição e resultado esperado são obrigatórios.", null));
        }

        var produto = await _db.Produtos.FindAsync(request.ProdutoId);
        if (produto is null)
        {
            return Ok(new SugestaoResponse(true, "Produto inválido.", null));
        }

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return Ok(new SugestaoResponse(true, "Categoria inválida.", null));
        }

        var autor = await _db.Usuarios.FindAsync(CurrentUserId());
        if (autor is null)
        {
            return Ok(new SugestaoResponse(true, "Usuário não encontrado.", null));
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

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, autor.Id)));
    }

    /// <summary>Edição pela própria pessoa autora, permitida apenas enquanto a sugestão ainda está em moderação.</summary>
    [HttpPost("editar/{id}")]
    public async Task<ActionResult<SugestaoResponse>> Editar(int id, CreateSugestaoRequest request)
    {
        if (!CamposObrigatoriosValidos(request.Titulo, request.Descricao, request.ResultadoEsperado))
        {
            return Ok(new SugestaoResponse(true, "Título, descrição e resultado esperado são obrigatórios.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.AutorId != currentUserId)
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Ok(new SugestaoResponse(true, "Sugestão já foi moderada e não pode mais ser editada.", null));
        }

        var produto = await _db.Produtos.FindAsync(request.ProdutoId);
        if (produto is null)
        {
            return Ok(new SugestaoResponse(true, "Produto inválido.", null));
        }

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return Ok(new SugestaoResponse(true, "Categoria inválida.", null));
        }

        sugestao.Titulo = request.Titulo;
        sugestao.Descricao = request.Descricao;
        sugestao.ResultadoEsperado = request.ResultadoEsperado;
        sugestao.ProdutoId = request.ProdutoId;
        sugestao.Produto = produto;
        sugestao.CategoriaId = request.CategoriaId;
        sugestao.Categoria = categoria;

        await _db.SaveChangesAsync();

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    /// <summary>Aprova uma sugestão em moderação, publicando-a (regra 7.1 do PRD).</summary>
    [HttpPost("aprovar/{id}")]
    public async Task<ActionResult<SugestaoResponse>> Aprovar(int id)
    {
        if (!IsAdmin())
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Ok(new SugestaoResponse(true, "Sugestão já foi moderada.", null));
        }

        sugestao.Status = StatusSugestao.Publicada;
        sugestao.DataModeracao = DateTime.UtcNow;
        sugestao.ModeradorId = currentUserId;

        await _db.SaveChangesAsync();

        sugestao.Moderador = await _db.Usuarios.FindAsync(sugestao.ModeradorId);
        await _notificacaoService.NotificarAsync(sugestao.Autor!, TipoNotificacao.SugestaoAprovada, sugestao);

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    /// <summary>Rejeita uma sugestão em moderação, com justificativa (regra 7.1 do PRD).</summary>
    [HttpPost("rejeitar/{id}")]
    public async Task<ActionResult<SugestaoResponse>> Rejeitar(int id, RejeitarSugestaoRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            return Ok(new SugestaoResponse(true, "Motivo é obrigatório.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.EmModeracao)
        {
            return Ok(new SugestaoResponse(true, "Sugestão já foi moderada.", null));
        }

        sugestao.Status = StatusSugestao.Rejeitada;
        sugestao.DataModeracao = DateTime.UtcNow;
        sugestao.MotivoRejeicao = request.Motivo;
        sugestao.ModeradorId = currentUserId;

        await _db.SaveChangesAsync();

        sugestao.Moderador = await _db.Usuarios.FindAsync(sugestao.ModeradorId);
        await _notificacaoService.NotificarAsync(sugestao.Autor!, TipoNotificacao.SugestaoRejeitada, sugestao);

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    /// <summary>
    /// Define o estágio de roadmap de uma sugestão já publicada (PRD, ponto em aberto #2). Notifica o
    /// autor por e-mail só na transição pra "Lançado".
    /// </summary>
    [HttpPost("roadmap/{id}")]
    public async Task<ActionResult<SugestaoResponse>> Roadmap(int id, AtualizarEstagioRoadmapRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Ok(new SugestaoResponse(true, "Só é possível definir o estágio de roadmap de sugestões publicadas.", null));
        }

        var estagioAnterior = sugestao.EstagioRoadmap;
        sugestao.EstagioRoadmap = request.Estagio;
        await _db.SaveChangesAsync();

        if (request.Estagio == EstagioRoadmap.Lancado && estagioAnterior != EstagioRoadmap.Lancado)
        {
            await _notificacaoService.NotificarAsync(sugestao.Autor!, TipoNotificacao.SugestaoLancada, sugestao);
        }

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    /// <summary>Vota em uma sugestão publicada. Limite de 3 votos ativos por cliente (regra 7.2 do PRD).</summary>
    [HttpPost("votar/{id}")]
    public async Task<ActionResult<SugestaoResponse>> Votar(int id)
    {
        if (!IsCliente())
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Ok(new SugestaoResponse(true, "Só é possível votar em sugestões publicadas.", null));
        }

        if (sugestao.EstagioRoadmap == EstagioRoadmap.Lancado)
        {
            return Ok(new SugestaoResponse(true, "Esta sugestão já foi lançada e não aceita mais votos.", null));
        }

        if (sugestao.Votos.Any(v => v.UsuarioId == currentUserId))
        {
            return Ok(new SugestaoResponse(true, "Você já votou nesta sugestão.", null));
        }

        var votosAtivos = await _db.Votos.CountAsync(v => v.UsuarioId == currentUserId);
        if (votosAtivos >= LimiteVotosPorUsuario)
        {
            return Ok(new SugestaoResponse(true, $"Limite de {LimiteVotosPorUsuario} votos ativos atingido. Remova um voto para votar em outra sugestão.", null));
        }

        sugestao.Votos.Add(new Voto { SugestaoId = sugestao.Id, UsuarioId = currentUserId });
        await _db.SaveChangesAsync();

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    /// <summary>Remove o voto do usuário autenticado nesta sugestão — usado para realocar o voto (regra 7.2 do PRD).</summary>
    [HttpPost("removervoto/{id}")]
    public async Task<ActionResult<SugestaoResponse>> RemoverVoto(int id)
    {
        if (!IsCliente())
        {
            return Ok(new SugestaoResponse(true, "Operação não permitida.", null));
        }

        var currentUserId = CurrentUserId();
        var sugestao = await CarregarSugestaoAsync(id);

        if (sugestao is null)
        {
            return Ok(new SugestaoResponse(true, "Sugestão não encontrada.", null));
        }

        var voto = sugestao.Votos.FirstOrDefault(v => v.UsuarioId == currentUserId);
        if (voto is null)
        {
            return Ok(new SugestaoResponse(true, "Você não tem voto nesta sugestão.", null));
        }

        sugestao.Votos.Remove(voto);
        _db.Votos.Remove(voto);
        await _db.SaveChangesAsync();

        return Ok(new SugestaoResponse(false, null, ToDto(sugestao, currentUserId)));
    }

    private async Task<Sugestao?> CarregarSugestaoAsync(int id) =>
        await _db.Sugestoes
            .Include(s => s.Produto)
            .Include(s => s.Categoria)
            .Include(s => s.Autor)
            .Include(s => s.Votos)
            .FirstOrDefaultAsync(s => s.Id == id);

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => User.IsInRole(nameof(RoleUsuario.AdminInterno));

    private bool IsCliente() => User.IsInRole(nameof(RoleUsuario.Cliente));

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
        s.EstagioRoadmap,
        s.DataCriacao,
        s.Votos.Count,
        s.Votos.Any(v => v.UsuarioId == currentUserId),
        s.DataModeracao,
        s.MotivoRejeicao,
        s.Moderador?.Nome);
}
