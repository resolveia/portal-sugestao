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

    /// <summary>Lista produtos (ERPs) ativos — usado no seletor de produto ao criar/editar sugestão.</summary>
    [HttpPost("listar")]
    public async Task<ActionResult<ListarProdutosResponse>> Listar()
    {
        var produtos = await _db.Produtos
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Ativo))
            .ToListAsync();

        return Ok(new ListarProdutosResponse(false, null, produtos));
    }

    /// <summary>Lista todos os produtos, ativos e inativos — tela de gestão de produtos (Admin).</summary>
    [HttpPost("listartodos")]
    public async Task<ActionResult<ListarProdutosResponse>> ListarTodos()
    {
        if (!IsAdmin())
        {
            return Ok(new ListarProdutosResponse(true, "Operação não permitida.", null));
        }

        var produtos = await _db.Produtos
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Ativo))
            .ToListAsync();

        return Ok(new ListarProdutosResponse(false, null, produtos));
    }

    [HttpPost("salvar")]
    public async Task<ActionResult<ProdutoResponse>> Salvar(CreateProdutoRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new ProdutoResponse(true, "Operação não permitida.", null));
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Ok(new ProdutoResponse(true, "Nome é obrigatório.", null));
        }

        var produto = new Produto { Nome = request.Nome, Ativo = true };
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        return Ok(new ProdutoResponse(false, null, new ProdutoDto(produto.Id, produto.Nome, produto.Ativo)));
    }

    /// <summary>Renomeia um produto existente.</summary>
    [HttpPost("editar/{id}")]
    public async Task<ActionResult<ProdutoResponse>> Editar(int id, CreateProdutoRequest request)
    {
        if (!IsAdmin())
        {
            return Ok(new ProdutoResponse(true, "Operação não permitida.", null));
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Ok(new ProdutoResponse(true, "Nome é obrigatório.", null));
        }

        var produto = await _db.Produtos.FindAsync(id);
        if (produto is null)
        {
            return Ok(new ProdutoResponse(true, "Produto não encontrado.", null));
        }

        produto.Nome = request.Nome;
        await _db.SaveChangesAsync();

        return Ok(new ProdutoResponse(false, null, new ProdutoDto(produto.Id, produto.Nome, produto.Ativo)));
    }

    /// <summary>Desativa um produto (exclusão lógica — a FK de Sugestao.ProdutoId impede exclusão física).</summary>
    [HttpPost("remover/{id}")]
    public async Task<ActionResult<ProdutoResponse>> Remover(int id)
    {
        if (!IsAdmin())
        {
            return Ok(new ProdutoResponse(true, "Operação não permitida.", null));
        }

        var produto = await _db.Produtos.FindAsync(id);
        if (produto is null)
        {
            return Ok(new ProdutoResponse(true, "Produto não encontrado.", null));
        }

        produto.Ativo = false;
        await _db.SaveChangesAsync();

        return Ok(new ProdutoResponse(false, null, null));
    }

    private bool IsAdmin() => User.IsInRole(nameof(RoleUsuario.AdminInterno));
}
