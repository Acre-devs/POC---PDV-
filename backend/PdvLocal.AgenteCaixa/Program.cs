using Microsoft.EntityFrameworkCore;
using PdvLocal.AgenteCaixa.Data;
using PdvLocal.AgenteCaixa.Services;

var builder = WebApplication.CreateBuilder(args);

// Escutar na porta 5001 (Agente Caixa Local)
builder.WebHost.UseUrls("http://0.0.0.0:5001");

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}
var dbPath = Path.Combine(dataDirectory, "pdv_caixa.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<AgenteCaixaDbContext>(options =>
    options.UseSqlite(connectionString));

// Registrar HttpClient apontando para o Servidor Central (Porta 5000)
builder.Services.AddHttpClient("ServidorCentral", client =>
{
    client.BaseAddress = new Uri("http://192.168.100.208:5000");
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Registrar Serviço de Sincronização em Segundo Plano
builder.Services.AddSingleton<SincronizacaoBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SincronizacaoBackgroundService>());

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AgenteCaixaDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("AllowAll");

// Servir arquivos estáticos do Angular (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

// Fallback SPA para Angular
app.MapFallbackToFile("index.html");

app.Run();
