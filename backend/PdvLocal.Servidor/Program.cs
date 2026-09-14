using Microsoft.EntityFrameworkCore;
using PdvLocal.Servidor.Data;

var builder = WebApplication.CreateBuilder(args);

// Escutar em todas as interfaces de rede na porta 5000 (Servidor Central)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}
var dbPath = Path.Combine(dataDirectory, "servidor_loja.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<ServidorDbContext>(options =>
    options.UseSqlite(connectionString));

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
    var dbContext = scope.ServiceProvider.GetRequiredService<ServidorDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();

app.Run();
