using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdvLocal.Application.DTOs;
using PdvLocal.Domain.Entities;
using PdvLocal.Servidor.Data;

namespace PdvLocal.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ServidorDbContext _context;

    public ProdutosController(ServidorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos()
    {
        var produtos = await _context.Produtos
            .AsNoTracking()
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo))
            .ToListAsync();

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> GetProdutoById(int id)
    {
        var p = await _context.Produtos.FindAsync(id);
        if (p == null) return NotFound(new { mensagem = "Produto não encontrado no Servidor Central." });
        return Ok(new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo));
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> CreateProduto([FromBody] CreateProdutoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome) || dto.Preco <= 0)
        {
            return BadRequest(new { mensagem = "Nome e Preço válidos são obrigatórios." });
        }

        var p = new Produto { Nome = dto.Nome, Preco = dto.Preco, Ativo = dto.Ativo };
        _context.Produtos.Add(p);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProdutoById), new { id = p.Id }, new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo));
    }
}
