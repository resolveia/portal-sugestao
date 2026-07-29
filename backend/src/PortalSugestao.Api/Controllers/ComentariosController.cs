using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/sugestoes/{sugestaoId}/comentarios")]
[Authorize]
public class ComentariosController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;

    public ComentariosController(PortalSugestaoDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista a thread de comentários, disponível apenas em sugestões publicadas (regra 7.3 do PRD).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComentarioDto>>> Listar(int sugestaoId)
    {
        var sugestao = await _db.Sugestoes.FindAsync(sugestaoId);
        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Conflict("Comentários disponíveis apenas em sugestões publicadas.");
        }

        var comentarios = await _db.Comentarios
            .Where(c => c.SugestaoId == sugestaoId)
            .Include(c => c.Usuario)
            .OrderBy(c => c.DataCriacao)
            .Select(c => new ComentarioDto(c.Id, c.SugestaoId, c.UsuarioId, c.Usuario!.Nome, c.Texto, c.DataCriacao))
            .ToListAsync();

        return Ok(comentarios);
    }

    /// <summary>
    /// Cria um comentário. Cliente e Admin podem comentar (tabela de permissões, seção 6 do PRD).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ComentarioDto>> Criar(int sugestaoId, CreateComentarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return BadRequest("Texto é obrigatório.");
        }

        var sugestao = await _db.Sugestoes.FindAsync(sugestaoId);
        if (sugestao is null)
        {
            return NotFound();
        }

        if (sugestao.Status != StatusSugestao.Publicada)
        {
            return Conflict("Só é possível comentar em sugestões publicadas.");
        }

        var currentUserId = CurrentUserId();
        var autor = await _db.Usuarios.FindAsync(currentUserId);
        if (autor is null)
        {
            return Unauthorized();
        }

        var comentario = new Comentario
        {
            SugestaoId = sugestaoId,
            UsuarioId = currentUserId,
            Texto = request.Texto
        };

        _db.Comentarios.Add(comentario);
        await _db.SaveChangesAsync();

        var dto = new ComentarioDto(comentario.Id, sugestaoId, currentUserId, autor.Nome, comentario.Texto, comentario.DataCriacao);
        return CreatedAtAction(nameof(Listar), new { sugestaoId }, dto);
    }

    /// <summary>
    /// Remove um comentário (moderação reativa — só Admin, tabela de permissões seção 6 do PRD).
    /// </summary>
    [HttpDelete("{comentarioId}")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<IActionResult> Remover(int sugestaoId, int comentarioId)
    {
        var comentario = await _db.Comentarios.FirstOrDefaultAsync(c => c.Id == comentarioId && c.SugestaoId == sugestaoId);
        if (comentario is null)
        {
            return NotFound();
        }

        _db.Comentarios.Remove(comentario);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
