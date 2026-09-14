using PdvLocal.Application.DTOs;

namespace PdvLocal.Application.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> ListarTodosAsync();
    Task<ProdutoDto?> ObterPorIdAsync(int id);
    Task<ProdutoDto> CriarAsync(CreateProdutoDto dto);
}
