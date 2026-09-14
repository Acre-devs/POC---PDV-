using Microsoft.EntityFrameworkCore;
using PdvLocal.Domain.Entities;

namespace PdvLocal.Infrastructure.Data;

public class PdvDbContext : DbContext
{
    public PdvDbContext(DbContextOptions<PdvDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

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
    }
}
