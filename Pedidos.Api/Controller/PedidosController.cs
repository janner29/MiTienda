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
    public PedidosController(PedidosDbContext db) => _db = db;

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

    // POST: crea el pedido junto con sus detalles. El total se calcula aquí.
    [HttpPost]
    public async Task<ActionResult<Pedido>> Post(Pedido pedido)
    {
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente))
            return BadRequest("El cliente no existe");

        if (pedido.Detalles == null || pedido.Detalles.Count == 0)
            return BadRequest("El pedido debe tener al menos un detalle");

        if (pedido.Detalles.Any(d => d.Cantidad <= 0 || d.PrecioUnitario < 0))
            return BadRequest("Cantidad y precio deben ser válidos");

        pedido.IdPedido = 0;
        pedido.Fecha = DateTime.Now;
        pedido.Estado = "Pendiente";
        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        foreach (var d in pedido.Detalles)
        {
            d.IdDetallePedido = 0;
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