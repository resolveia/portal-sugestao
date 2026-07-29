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
[Route("api/sugestoes")]
[Authorize]
public class SugestoesController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;

    public SugestoesController(PortalSugestaoDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista sugestões publicadas, ordenadas por número de votos (ranking — seção 5.4 do PRD).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SugestaoDto>>> Listar()
    {
        var sugestoes = await _db.Sugestoes
            .Where(s => s.Status == StatusSugestao.Publicada)
            .OrderByDescending(s => s.Votos.Count)
            .Select(s => new SugestaoDto(
                s.Id,
                s.Titulo,
                s.Descricao,
                s.CategoriaId,
                s.Categoria!.Nome,
                s.AutorId,
                s.Autor!.Nome,
                s.Status,
                s.DataCriacao,
                s.Votos.Count))
            .ToListAsync();

        return Ok(sugestoes);
    }

    /// <summary>
    /// Cadastra uma nova sugestão, sempre entrando com status "Em moderação" (regra 7.1 do PRD).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SugestaoDto>> Criar(CreateSugestaoRequest request)
    {
        var autorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var categoria = await _db.Categorias.FindAsync(request.CategoriaId);
        if (categoria is null)
        {
            return BadRequest("Categoria inválida.");
        }

        var autor = await _db.Usuarios.FindAsync(autorId);
        if (autor is null)
        {
            return Unauthorized();
        }

        var sugestao = new Sugestao
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            CategoriaId = request.CategoriaId,
            AutorId = autorId,
            Status = StatusSugestao.EmModeracao
        };

        _db.Sugestoes.Add(sugestao);
        await _db.SaveChangesAsync();

        var dto = new SugestaoDto(
            sugestao.Id, sugestao.Titulo, sugestao.Descricao,
            sugestao.CategoriaId, categoria.Nome,
            sugestao.AutorId, autor.Nome,
            sugestao.Status, sugestao.DataCriacao, TotalVotos: 0);

        return CreatedAtAction(nameof(Listar), dto);
    }
}
