using Microsoft.EntityFrameworkCore;
using PdvLocal.Domain.Entities;

namespace PdvLocal.Servidor.Data;

public class ServidorDbContext : DbContext
{
    public ServidorDbContext(DbContextOptions<ServidorDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Preco).HasConversion<double>();
            entity.Property(p => p.Ativo).HasDefaultValue(true);

            entity.HasData(
                new Produto { Id = 1, Nome = "Café Expresso 50ml", Preco = 5.50m, Ativo = true },
                new Produto { Id = 2, Nome = "Pão na Chapa com Manteiga", Preco = 7.00m, Ativo = true },
                new Produto { Id = 3, Nome = "Suco Natural de Laranja 500ml", Preco = 10.00m, Ativo = true },
                new Produto { Id = 4, Nome = "Salgado Assado Frango", Preco = 8.50m, Ativo = true },
                new Produto { Id = 5, Nome = "Refrigerante Lata 350ml", Preco = 6.00m, Ativo = true }
            );
        });

        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.HasIndex(v => v.CodigoVenda).IsUnique();
            entity.Property(v => v.Total).HasConversion<double>();
            entity.HasMany(v => v.Itens).WithOne().HasForeignKey(i => i.VendaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.PrecoUnitario).HasConversion<double>();
            entity.Property(i => i.Subtotal).HasConversion<double>();
        });
    }
}
