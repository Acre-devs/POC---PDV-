using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdvLocal.AgenteCaixa.Data;
using PdvLocal.AgenteCaixa.Services;

namespace PdvLocal.AgenteCaixa.Controllers;

[ApiController]
[Route("api/status-sincronizacao")]
public class StatusSincronizacaoController : ControllerBase
{
    private readonly AgenteCaixaDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SincronizacaoBackgroundService _syncService;

    public StatusSincronizacaoController(
        AgenteCaixaDbContext context,
        IHttpClientFactory httpClientFactory,
        SincronizacaoBackgroundService syncService)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        bool centralOnline = false;
        try
        {
            var client = _httpClientFactory.CreateClient("ServidorCentral");
            var res = await client.GetAsync("/api/health");
            centralOnline = res.IsSuccessStatusCode;
        }
        catch
        {
            centralOnline = false;
        }

        int pendentesCount = await _context.Vendas.CountAsync(v => !v.Sincronizado);
        var ultimaSinc = await _context.Vendas
            .Where(v => v.Sincronizado && v.DataSincronizacao.HasValue)
            .MaxAsync(v => (DateTime?)v.DataSincronizacao);

        string mensagem = centralOnline
            ? (pendentesCount > 0 ? $"{pendentesCount} venda(s) aguardando sincronização." : "Servidor Central Conectado. Todas as vendas sincronizadas.")
            : "MODO CONTINGÊNCIA: Servidor Central Offline. Vendas armazenadas localmente.";

        // Retorna o objeto anônimo formatado com as chaves exatas que o Angular espera
        return Ok(new
        {
            servidorCentralOnline = centralOnline,
            vendasPendentesCount = pendentesCount,
            ultimaSincronizacao = ultimaSinc,
            mensagem = mensagem
        });
    }

    [HttpPost("sincronizar-agora")]
    public async Task<IActionResult> SincronizarAgora()
    {
        int sincronizadas = await _syncService.SincronizarVendasPendentesAsync();
        return Ok(new { sincronizadas, mensagem = $"{sincronizadas} venda(s) sincronizada(s) com o Servidor Central." });
    }
}