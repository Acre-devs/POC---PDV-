using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdvLocal.AgenteCaixa.Data;
using PdvLocal.AgenteCaixa.Services;
using PdvLocal.Application.DTOs;
using PdvLocal.Domain.Entities;

namespace PdvLocal.AgenteCaixa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly AgenteCaixaDbContext _context;
    private readonly SincronizacaoBackgroundService _syncService;

    public VendasController(AgenteCaixaDbContext context, SincronizacaoBackgroundService syncService)
    {
        _context = context;
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasLocais()
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

    [HttpPost]
    public async Task<ActionResult<VendaDto>> RealizarVenda([FromBody] CriarVendaDto dto)
    {
        if (dto.Itens == null || !dto.Itens.Any())
        {
            return BadRequest(new { mensagem = "A venda deve conter pelo menos um item." });
        }

        var vendaLocal = new Venda
        {
            CodigoVenda = string.IsNullOrWhiteSpace(dto.CodigoVenda) ? "V-" + Guid.NewGuid().ToString("N")[..8].ToUpper() : dto.CodigoVenda,
            DataCriacao = DateTime.Now,
            Total = dto.Total,
            Sincronizado = false,
            Itens = dto.Itens.Select(i => new ItemVenda
            {
                ProdutoId = i.ProdutoId,
                NomeProduto = i.NomeProduto,
                PrecoUnitario = i.PrecoUnitario,
                Quantidade = i.Quantidade,
                Subtotal = i.Subtotal
            }).ToList()
        };

        _context.Vendas.Add(vendaLocal);
        await _context.SaveChangesAsync();

        // Tentar sincronização em segundo plano imediatamente
        _ = _syncService.SincronizarVendasPendentesAsync();

        var resultDto = new VendaDto(
            vendaLocal.Id,
            vendaLocal.CodigoVenda,
            vendaLocal.DataCriacao,
            vendaLocal.Total,
            vendaLocal.Sincronizado,
            vendaLocal.DataSincronizacao,
            vendaLocal.Itens.Select(i => new ItemVendaDto(i.ProdutoId, i.NomeProduto, i.PrecoUnitario, i.Quantidade, i.Subtotal)).ToList()
        );

        return Ok(resultDto);
    }
}
