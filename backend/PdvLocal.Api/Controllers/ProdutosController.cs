using Microsoft.AspNetCore.Mvc;
using PdvLocal.Application.DTOs;
using PdvLocal.Application.Interfaces;

namespace PdvLocal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos()
    {
        var produtos = await _produtoService.ListarTodosAsync();
        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> GetProdutoById(int id)
    {
        var produto = await _produtoService.ObterPorIdAsync(id);
        if (produto == null)
        {
            return NotFound(new { mensagem = $"Produto com ID {id} não encontrado." });
        }
        return Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> CreateProduto([FromBody] CreateProdutoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            return BadRequest(new { mensagem = "O nome do produto é obrigatório." });
        }

        if (dto.Preco <= 0)
        {
            return BadRequest(new { mensagem = "O preço do produto deve ser maior que zero." });
        }

        var produtoCriado = await _produtoService.CriarAsync(dto);
        return CreatedAtAction(nameof(GetProdutoById), new { id = produtoCriado.Id }, produtoCriado);
    }
}
