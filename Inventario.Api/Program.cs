using Inventario.Api.Data;
using Inventario.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventarioDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("InventarioDb")));

builder.Services.AddHttpClient<CatalogoClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Servicios:Catalogo"]!);
    c.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aplica las migraciones al iniciar (crea/actualiza la BD)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
    for (var intento = 1; intento <= 10; intento++)
    {
        try { db.Database.Migrate(); break; }
        catch when (intento < 10) { Thread.Sleep(3000); }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();