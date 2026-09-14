namespace PdvLocal.Domain.Entities;

public class Venda
{
    public int Id { get; set; }
    public string CodigoVenda { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public bool Sincronizado { get; set; } = false;
    public DateTime? DataSincronizacao { get; set; }
    public List<ItemVenda> Itens { get; set; } = new();
}
