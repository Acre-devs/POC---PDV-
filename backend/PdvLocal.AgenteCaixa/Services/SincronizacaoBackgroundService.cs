using Microsoft.EntityFrameworkCore;
using PdvLocal.AgenteCaixa.Data;
using PdvLocal.Application.DTOs;

namespace PdvLocal.AgenteCaixa.Services;

public class SincronizacaoBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SincronizacaoBackgroundService> _logger;

    public SincronizacaoBackgroundService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<SincronizacaoBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Sincronização em Segundo Plano Iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SincronizarVendasPendentesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Tentativa de sincronização em segundo plano: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public async Task<int> SincronizarVendasPendentesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AgenteCaixaDbContext>();
        var client = _httpClientFactory.CreateClient("ServidorCentral");

        // 1. Verificar se o Servidor Central está acessível
        HttpResponseMessage healthRes;
        try
        {
            healthRes = await client.GetAsync("/api/health");
            if (!healthRes.IsSuccessStatusCode)
            {
                return 0;
            }
        }
        catch
        {
            // Servidor Central offline
            return 0;
        }

        // 2. Buscar vendas pendentes no banco local
        var vendasPendentes = await dbContext.Vendas
            .Include(v => v.Itens)
            .Where(v => !v.Sincronizado)
            .ToListAsync();

        if (!vendasPendentes.Any())
        {
            return 0;
        }

        // 3. Montar DTOs para envio em lote
        var lote = vendasPendentes.Select(v => new CriarVendaDto(
            v.CodigoVenda,
            v.Total,
            v.Itens.Select(i => new ItemVendaDto(i.ProdutoId, i.NomeProduto, i.PrecoUnitario, i.Quantidade, i.Subtotal)).ToList()
        )).ToList();

        // 4. Enviar lote para o Servidor Central
        var response = await client.PostAsJsonAsync("/api/sincronizacao/vendas", lote);
        if (response.IsSuccessStatusCode)
        {
            var agora = DateTime.Now;
            foreach (var v in vendasPendentes)
            {
                v.Sincronizado = true;
                v.DataSincronizacao = agora;
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"{vendasPendentes.Count} venda(s) sincronizada(s) com sucesso!");
            return vendasPendentes.Count;
        }

        return 0;
    }
}
