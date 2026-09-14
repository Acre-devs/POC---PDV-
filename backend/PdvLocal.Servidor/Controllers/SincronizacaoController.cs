using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdvLocal.Application.DTOs;
using PdvLocal.Domain.Entities;
using PdvLocal.Servidor.Data;

namespace PdvLocal.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SincronizacaoController : ControllerBase
{
    private readonly ServidorDbContext _context;

    public SincronizacaoController(ServidorDbContext context)
    {
        _context = context;
    }

    [HttpPost("vendas")]
    public async Task<IActionResult> ReceberVendas([FromBody] List<CriarVendaDto> loteVendas)
    {
        if (loteVendas == null || !loteVendas.Any())
        {
            return Ok(new { sincronizadas = 0, mensagem = "Nenhuma venda para sincronizar." });
        }

        int quantidadeSincronizada = 0;

        foreach (var dto in loteVendas)
        {
            var existe = await _context.Vendas.AnyAsync(v => v.CodigoVenda == dto.CodigoVenda);
            if (!existe)
            {
                var vendaCentral = new Venda
                {
                    CodigoVenda = dto.CodigoVenda,
                    DataCriacao = DateTime.Now,
                    Total = dto.Total,
                    Sincronizado = true,
                    DataSincronizacao = DateTime.Now,
                    Itens = dto.Itens.Select(i => new ItemVenda
                    {
                        ProdutoId = i.ProdutoId,
                        NomeProduto = i.NomeProduto,
                        PrecoUnitario = i.PrecoUnitario,
                        Quantidade = i.Quantidade,
                        Subtotal = i.Subtotal
                    }).ToList()
                };

                _context.Vendas.Add(vendaCentral);
                quantidadeSincronizada++;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            sincronizadas = quantidadeSincronizada,
            mensagem = $"{quantidadeSincronizada} venda(s) consolidadas com sucesso no Servidor Central."
        });
    }

    [HttpGet("vendas")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> ObterVendasConsolidadas()
    {
        var vendas = await _context.Vendas
            .Include(v => v.Itens)
            .AsNoTracking()
            .OrderByDescending(v => v.DataCriacao)
            .Select(v => new VendaDto(
                v.Id,
                v.CodigoVenda,
                v.DataCriacao,
                v.Total,
                v.Sincronizado,
                v.DataSincronizacao,
                v.Itens.Select(i => new ItemVendaDto(i.ProdutoId, i.NomeProduto, i.PrecoUnitario, i.Quantidade, i.Subtotal)).ToList()
            ))
            .ToListAsync();

        return Ok(vendas);
    }
}
