namespace PdvLocal.Application.DTOs;

public record ProdutoDto(int Id, string Nome, decimal Preco, bool Ativo);

public record CreateProdutoDto(string Nome, decimal Preco, bool Ativo = true);
