namespace PdvLocal.Application.DTOs;

public record ItemVendaDto(int ProdutoId, string NomeProduto, decimal PrecoUnitario, int Quantidade, decimal Subtotal);

public record VendaDto(
    int Id,
    string CodigoVenda,
    DateTime DataCriacao,
    decimal Total,
    bool Sincronizado,
    DateTime? DataSincronizacao,
    List<ItemVendaDto> Itens
);

public record CriarVendaDto(
    string? CodigoVenda,
    decimal Total,
    List<ItemVendaDto> Itens
);

public record StatusSincronizacaoDto(
    bool ServidorCentralOnline,
    int VendasPendentesCount,
    DateTime? UltimaSincronizacao,
    string Mensagem
);
