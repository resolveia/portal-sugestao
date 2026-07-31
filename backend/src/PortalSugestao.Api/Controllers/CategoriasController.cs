using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;

    public CategoriasController(PortalSugestaoDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista categorias ativas — usado no seletor de categoria ao criar/editar sugestão.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> Listar()
    {
        var categorias = await _db.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Ativo))
            .ToListAsync();

        return Ok(categorias);
    }

    /// <summary>
    /// Lista todas as categorias, ativas e inativas — usado na tela de gestão de categorias (Admin).
    /// </summary>
    [HttpGet("todas")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ListarTodas()
    {
        var categorias = await _db.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Ativo))
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<CategoriaDto>> Criar(CreateCategoriaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest("Nome é obrigatório.");
        }

        var categoria = new Categoria { Nome = request.Nome, Ativo = true };
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        var dto = new CategoriaDto(categoria.Id, categoria.Nome, categoria.Ativo);
        return CreatedAtAction(nameof(Listar), dto);
    }

    /// <summary>
    /// Renomeia uma categoria existente.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<CategoriaDto>> Editar(int id, CreateCategoriaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest("Nome é obrigatório.");
        }

        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound();
        }

        categoria.Nome = request.Nome;
        await _db.SaveChangesAsync();

        return Ok(new CategoriaDto(categoria.Id, categoria.Nome, categoria.Ativo));
    }

    /// <summary>
    /// Desativa uma categoria (exclusão lógica — a FK de Sugestao.CategoriaId impede exclusão física).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<IActionResult> Remover(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound();
        }

        categoria.Ativo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
