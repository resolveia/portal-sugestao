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
[Route("api/sugestoes/{sugestaoId}/comentarios")]
[Authorize]
public class ComentariosController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;
    private readonly NotificacaoService _notificacaoService;

    public ComentariosController(PortalSugestaoDbContext db, NotificacaoService notificacaoService)
    {
        _db = db;
        _notificacaoService = notificacaoService;
    }

    /// <summary>Lista a thread de comentários, disponível apenas em sugestões publicadas (regra 7.3 do PRD).</summary>
    [HttpPost("listar")]
    public async Task<ActionResult<ListarComentariosResponse>> Listar(int sugestaoId)
    {
        var sugestao = await _db.Sugestoes.FindAsync(sugestaoId);
        if (sugestao is null)
        {
            return Ok(new ListarComentariosResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Ok(new ListarComentariosResponse(true, "Comentários disponíveis apenas em sugestões publicadas.", null));
        }

        var comentarios = await _db.Comentarios
            .Where(c => c.SugestaoId == sugestaoId)
            .Include(c => c.Usuario)
            .OrderBy(c => c.DataCriacao)
            .Select(c => new ComentarioDto(c.Id, c.SugestaoId, c.UsuarioId, c.Usuario!.Nome, c.Texto, c.DataCriacao))
            .ToListAsync();

        return Ok(new ListarComentariosResponse(false, null, comentarios));
    }

    /// <summary>Cria um comentário. Cliente e Admin podem comentar (tabela de permissões, seção 6 do PRD).</summary>
    [HttpPost("salvar")]
    public async Task<ActionResult<ComentarioResponse>> Salvar(int sugestaoId, CreateComentarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return Ok(new ComentarioResponse(true, "Texto é obrigatório.", null));
        }

        var sugestao = await _db.Sugestoes.FindAsync(sugestaoId);
        if (sugestao is null)
        {
            return Ok(new ComentarioResponse(true, "Sugestão não encontrada.", null));
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Ok(new ComentarioResponse(true, "Só é possível comentar em sugestões publicadas.", null));
        }

        var currentUserId = CurrentUserId();
        var autor = await _db.Usuarios.FindAsync(currentUserId);
        if (autor is null)
        {
            return Ok(new ComentarioResponse(true, "Usuário não encontrado.", null));
        }

        var comentario = new Comentario
        {
            SugestaoId = sugestaoId,
            UsuarioId = currentUserId,
            Texto = request.Texto
        };

        _db.Comentarios.Add(comentario);
        await _db.SaveChangesAsync();

        // Notifica o autor da sugestão só quando quem responde é o Admin — evita notificar
        // por comentários de outros clientes (PRD, ponto em aberto #3).
        if (autor.Role == RoleUsuario.AdminInterno && sugestao.AutorId != currentUserId)
        {
            var autorSugestao = await _db.Usuarios.FindAsync(sugestao.AutorId);
            if (autorSugestao is not null)
            {
                await _notificacaoService.NotificarAsync(autorSugestao, TipoNotificacao.NovoComentario, sugestao);
            }
        }

        var dto = new ComentarioDto(comentario.Id, sugestaoId, currentUserId, autor.Nome, comentario.Texto, comentario.DataCriacao);
        return Ok(new ComentarioResponse(false, null, dto));
    }

    /// <summary>Remove um comentário (moderação reativa — só Admin, tabela de permissões seção 6 do PRD).</summary>
    [HttpPost("remover/{comentarioId}")]
    public async Task<ActionResult<RemoverComentarioResponse>> Remover(int sugestaoId, int comentarioId)
    {
        if (!User.IsInRole(nameof(RoleUsuario.AdminInterno)))
        {
            return Ok(new RemoverComentarioResponse(true, "Operação não permitida."));
        }

        var comentario = await _db.Comentarios.FirstOrDefaultAsync(c => c.Id == comentarioId && c.SugestaoId == sugestaoId);
        if (comentario is null)
        {
            return Ok(new RemoverComentarioResponse(true, "Comentário não encontrado."));
        }

        _db.Comentarios.Remove(comentario);
        await _db.SaveChangesAsync();

        return Ok(new RemoverComentarioResponse(false, null));
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
