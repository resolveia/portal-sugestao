using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

/// <summary>
/// Contrato alinhado à convenção da plataforma (docs/autenticacao-e-api-portal-sugestoes.md,
/// docs/api-contract.md): tudo POST, resposta sempre {Erro, Mensagem?, ...dados}, HTTP 200 mesmo
/// em erro de negócio/permissão — só falhas técnicas (ex.: não autenticado) usam status HTTP real.
/// </summary>
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

    /// <summary>Lista categorias ativas — usado no seletor de categoria ao criar/editar sugestão.</summary>
    [HttpPost("listar")]
    public async Task<ActionResult<ListarCategoriasResponse>> Listar()
    {
        var categorias = await _db.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Ativo))
            .ToListAsync();

        return Ok(new ListarCategoriasResponse(false, null, categorias));
    }

    /// <summary>Lista todas as categorias, ativas e inativas — tela de gestão de categorias (Admin).</summary>
    [HttpPost("listartodas")]
    public async Task<ActionResult<ListarCategoriasResponse>> ListarTodas()
    {
        if (!IsAdmin())
        {
            return Ok(new ListarCategoriasResponse(true, "Operação não permitida.", null));
        }

        var categorias = await _db.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Ativo))
            .ToListAsync();

        return Ok(new ListarCategoriasResponse(false, null, categorias));
    }

    [HttpPost("salvar")]
    public async Task<ActionResult<CategoriaResponse>> Salvar(CreateCategoriaRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new CategoriaResponse(true, "Operação não permitida.", null));
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Ok(new CategoriaResponse(true, "Nome é obrigatório.", null));
        }

        var categoria = new Categoria { Nome = request.Nome, Ativo = true };
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return Ok(new CategoriaResponse(false, null, new CategoriaDto(categoria.Id, categoria.Nome, categoria.Ativo)));
    }

    /// <summary>Renomeia uma categoria existente.</summary>
    [HttpPost("editar/{id}")]
    public async Task<ActionResult<CategoriaResponse>> Editar(int id, CreateCategoriaRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new CategoriaResponse(true, "Operação não permitida.", null));
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Ok(new CategoriaResponse(true, "Nome é obrigatório.", null));
        }

        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return Ok(new CategoriaResponse(true, "Categoria não encontrada.", null));
        }

        categoria.Nome = request.Nome;
        await _db.SaveChangesAsync();

        return Ok(new CategoriaResponse(false, null, new CategoriaDto(categoria.Id, categoria.Nome, categoria.Ativo)));
    }

    /// <summary>Desativa uma categoria (exclusão lógica — a FK de Sugestao.CategoriaId impede exclusão física).</summary>
    [HttpPost("remover/{id}")]
    public async Task<ActionResult<CategoriaResponse>> Remover(int id)
    {
        if (!IsAdmin())
        {
            return Ok(new CategoriaResponse(true, "Operação não permitida.", null));
        }

        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return Ok(new CategoriaResponse(true, "Categoria não encontrada.", null));
        }

        categoria.Ativo = false;
        await _db.SaveChangesAsync();

        return Ok(new CategoriaResponse(false, null, null));
    }

    private bool IsAdmin() => User.IsInRole(nameof(RoleUsuario.AdminInterno));
}
