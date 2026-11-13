using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _db;
    private readonly IHttpClientFactory _http;

    public PedidosController(PedidosDbContext db, IHttpClientFactory http)
    {
        _db = db;
        _http = http;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> Get()
        => await _db.Pedidos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> Get(int id)
    {
        var pedido = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    // GET: api/Pedidos/cliente/1
    [HttpGet("cliente/{idCliente}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPorCliente(int idCliente)
        => await _db.Pedidos.AsNoTracking()
            .Include(p => p.Detalles)
            .Where(p => p.IdCliente == idCliente)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

    // POST: valida con Catalogo e Inventario, toma el precio del catálogo y guarda.
    [HttpPost]
    public async Task<ActionResult<Pedido>> Post(Pedido pedido)
    {
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente))
            return BadRequest("El cliente no existe");

        if (pedido.Detalles == null || pedido.Detalles.Count == 0)
            return BadRequest("El pedido debe tener al menos un detalle");

        if (pedido.Detalles.Any(d => d.Cantidad <= 0))
            return BadRequest("La cantidad de cada detalle debe ser mayor a 0");

        var catalogo = _http.CreateClient("Catalogo");
        var inventario = _http.CreateClient("Inventario");

        try
        {
            // 1) Pedidos -> Catalogo: el producto existe y está activo; el precio sale del catálogo
            foreach (var d in pedido.Detalles)
            {
                var resp = await catalogo.GetAsync($"api/Productos/{d.IdProducto}");
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return BadRequest($"El producto {d.IdProducto} no existe");
                resp.EnsureSuccessStatusCode();

                var producto = await resp.Content.ReadFromJsonAsync<ProductoDto>();
                if (producto is null)
                    return StatusCode(502, "Respuesta inválida de Catalogo.Api");
                if (producto.Estado != "Activo")
                    return BadRequest($"El producto {d.IdProducto} no está activo");

                d.PrecioUnitario = producto.Precio;
            }

            // 2) Pedidos -> Inventario: hay stock suficiente (se suman todas las bodegas)
            foreach (var grupo in pedido.Detalles.GroupBy(d => d.IdProducto))
            {
                var resp = await inventario.GetAsync($"api/Existencias/producto/{grupo.Key}");
                resp.EnsureSuccessStatusCode();

                var existencias = await resp.Content.ReadFromJsonAsync<List<ExistenciaDto>>()
                                  ?? new List<ExistenciaDto>();
                var disponible = existencias.Sum(e => e.Stock);
                var solicitado = grupo.Sum(d => d.Cantidad);

                if (solicitado > disponible)
                    return BadRequest(
                        $"Stock insuficiente para el producto {grupo.Key}. " +
                        $"Disponible: {disponible}, solicitado: {solicitado}");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            return StatusCode(503, "Catalogo.Api o Inventario.Api no está disponible");
        }

        pedido.IdPedido = 0;
        pedido.Fecha = DateTime.Now;
        pedido.Estado = "Pendiente";
        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        foreach (var d in pedido.Detalles)
        {
            d.IdDetalle = 0;
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = pedido.IdPedido }, pedido);
    }

    // PUT: api/Pedidos/5/cancelar
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido is null) return NotFound();
        if (pedido.Estado == "Cancelado") return BadRequest("El pedido ya está cancelado");

        pedido.Estado = "Cancelado";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido is null) return NotFound();
        _db.Pedidos.Remove(pedido);   // los detalles se borran en cascada
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// Solo los campos que Pedidos necesita leer de las otras APIs
internal record ProductoDto(decimal Precio, string Estado);
internal record ExistenciaDto(int Stock);