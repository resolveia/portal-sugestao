using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSugestao.Application.DTOs;
using PortalSugestao.Domain.Entities;
using PortalSugestao.Domain.Enums;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Api.Controllers;

[ApiController]
[Route("api/produtos")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly PortalSugestaoDbContext _db;

    public ProdutosController(PortalSugestaoDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista produtos (ERPs) ativos — usado no seletor de produto ao criar/editar sugestão.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> Listar()
    {
        var produtos = await _db.Produtos
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Ativo))
            .ToListAsync();

        return Ok(produtos);
    }

    /// <summary>
    /// Lista todos os produtos, ativos e inativos — usado na tela de gestão de produtos (Admin).
    /// </summary>
    [HttpGet("todos")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ListarTodos()
    {
        var produtos = await _db.Produtos
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Ativo))
            .ToListAsync();

        return Ok(produtos);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<ProdutoDto>> Criar(CreateProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest("Nome é obrigatório.");
        }

        var produto = new Produto { Nome = request.Nome, Ativo = true };
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        var dto = new ProdutoDto(produto.Id, produto.Nome, produto.Ativo);
        return CreatedAtAction(nameof(Listar), dto);
    }

    /// <summary>
    /// Renomeia um produto existente.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<ActionResult<ProdutoDto>> Editar(int id, CreateProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest("Nome é obrigatório.");
        }

        var produto = await _db.Produtos.FindAsync(id);
        if (produto is null)
        {
            return NotFound();
        }

        produto.Nome = request.Nome;
        await _db.SaveChangesAsync();

        return Ok(new ProdutoDto(produto.Id, produto.Nome, produto.Ativo));
    }

    /// <summary>
    /// Desativa um produto (exclusão lógica — a FK de Sugestao.ProdutoId impede exclusão física).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleUsuario.AdminInterno))]
    public async Task<IActionResult> Remover(int id)
    {
        var produto = await _db.Produtos.FindAsync(id);
        if (produto is null)
        {
            return NotFound();
        }

        produto.Ativo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
