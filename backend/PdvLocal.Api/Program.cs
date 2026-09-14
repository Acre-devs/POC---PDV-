using Microsoft.EntityFrameworkCore;
using PdvLocal.Application.Interfaces;
using PdvLocal.Infrastructure.Data;
using PdvLocal.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar binding para escutar em todas as interfaces de rede (0.0.0.0) na porta 5000
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Garantir diretório de dados persistentes local
var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}
var dbPath = Path.Combine(dataDirectory, "pdv.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<PdvDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IProdutoService, ProdutoService>();

builder.Services.AddControllers();

// Configuração de CORS para desenvolvimento local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Garantir que o banco de dados e dados iniciais (seeds) existam no startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PdvDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("AllowDevOrigin");

// Servir arquivos estáticos do Angular compilados na pasta wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Fallback para SPA (Angular): Redireciona requisições não-API para index.html
app.MapFallbackToFile("index.html");

app.Run();
