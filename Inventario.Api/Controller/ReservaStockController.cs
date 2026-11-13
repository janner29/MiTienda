using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservaStockController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public ReservaStockController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaStock>>> Get()
        => await _db.ReservasStock.AsNoTracking().Include(r => r.Bodega).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservaStock>> Get(int id)
    {
        var reserva = await _db.ReservasStock.AsNoTracking()
            .Include(r => r.Bodega)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
        return reserva is null ? NotFound() : Ok(reserva);
    }

    [HttpPost]
    public async Task<ActionResult<ReservaStock>> Post(ReservaStock reserva)
    {
        if (reserva.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0");

        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == reserva.IdBodega))
            return BadRequest("La bodega no existe");

        try
        {
            if (!await _catalogo.ProductoExisteAsync(reserva.IdProducto))
                return BadRequest("El producto no existe en Catalogo");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Catalogo.Api no está disponible");
        }

        var existencia = await _db.Existencias.AsNoTracking().FirstOrDefaultAsync(e =>
            e.IdBodega == reserva.IdBodega && e.IdProducto == reserva.IdProducto);

        if (existencia is null)
            return BadRequest("No hay existencias de ese producto en la bodega");

        var reservado = await _db.ReservasStock
            .Where(r => r.IdBodega == reserva.IdBodega
                     && r.IdProducto == reserva.IdProducto
                     && r.Estado == "Activa")
            .SumAsync(r => (int?)r.Cantidad) ?? 0;

        var disponible = existencia.Stock - reservado;
        if (reserva.Cantidad > disponible)
            return BadRequest($"Stock insuficiente. Disponible para reservar: {disponible}");

        reserva.IdReserva = 0;
        reserva.Estado = "Activa";
        reserva.Fecha = DateTime.Now;
        _db.ReservasStock.Add(reserva);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = reserva.IdReserva }, reserva);
    }

    // PUT: api/ReservaStock/5/cancelar
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reserva = await _db.ReservasStock.FindAsync(id);
        if (reserva is null) return NotFound();
        if (reserva.Estado == "Cancelada") return BadRequest("La reserva ya está cancelada");

        reserva.Estado = "Cancelada";
        await _db.SaveChangesAsync();
        return NoContent();
    }
}