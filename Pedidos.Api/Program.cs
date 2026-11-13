using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PedidosDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PedidosDb")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("Catalogo", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Servicios:Catalogo"]!);
    c.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient("Inventario", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Servicios:Inventario"]!);
    c.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Aplica las migraciones al iniciar (crea/actualiza la BD)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
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