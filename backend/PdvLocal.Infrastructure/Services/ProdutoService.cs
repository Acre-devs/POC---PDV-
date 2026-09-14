using Microsoft.EntityFrameworkCore;
using PdvLocal.Application.DTOs;
using PdvLocal.Application.Interfaces;
using PdvLocal.Domain.Entities;
using PdvLocal.Infrastructure.Data;

namespace PdvLocal.Infrastructure.Services;

public class ProdutoService : IProdutoService
{
    private readonly PdvDbContext _context;

    public ProdutoService(PdvDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProdutoDto>> ListarTodosAsync()
    {
        return await _context.Produtos
            .AsNoTracking()
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo))
            .ToListAsync();
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(int id)
    {
        var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null) return null;

        return new ProdutoDto(produto.Id, produto.Nome, produto.Preco, produto.Ativo);
    }

    public async Task<ProdutoDto> CriarAsync(CreateProdutoDto dto)
    {
        var produto = new Produto
        {
            Nome = dto.Nome,
            Preco = dto.Preco,
            Ativo = dto.Ativo
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return new ProdutoDto(produto.Id, produto.Nome, produto.Preco, produto.Ativo);
    }
}
