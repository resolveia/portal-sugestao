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

    [HttpPost]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<CategoriaDto>> Criar(CreateCategoriaRequest request)
    {
        var categoria = new Categoria { Nome = request.Nome, Ativo = true };
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        var dto = new CategoriaDto(categoria.Id, categoria.Nome, categoria.Ativo);
        return CreatedAtAction(nameof(Listar), dto);
    }
}
