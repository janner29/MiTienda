using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInvController : ControllerBase
{
    private readonly InventarioDbContext _db;
    public MovimientosInvController(InventarioDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoInv>>> Get()
        => await _db.MovimientosInv.AsNoTracking()
            .Include(m => m.Bodega)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<MovimientoInv>> Get(int id)
    {
        var mov = await _db.MovimientosInv.AsNoTracking()
            .Include(m => m.Bodega)
            .FirstOrDefaultAsync(m => m.IdMovimientoInv == id);
        return mov is null ? NotFound() : Ok(mov);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoInv>> Post(MovimientoInv mov)
    {
        if (mov.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0");

        if (mov.TipoMovimiento != "Entrada" && mov.TipoMovimiento != "Salida")
            return BadRequest("TipoMovimiento debe ser 'Entrada' o 'Salida'");

        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == mov.IdBodega))
            return BadRequest("La bodega no existe");

        var existencia = await _db.Existencias.FirstOrDefaultAsync(e =>
            e.IdBodega == mov.IdBodega && e.IdProducto == mov.IdProducto);

        if (existencia is null)
        {
            if (mov.TipoMovimiento == "Salida")
                return BadRequest("No hay existencias de ese producto en la bodega");

            existencia = new Existencia
            {
                IdBodega = mov.IdBodega,
                IdProducto = mov.IdProducto,
                Cantidad = 0,
                StockMinimo = 0
            };
            _db.Existencias.Add(existencia);
        }

        if (mov.TipoMovimiento == "Entrada")
        {
            existencia.Cantidad += mov.Cantidad;
        }
        else
        {
            if (existencia.Cantidad < mov.Cantidad)
                return BadRequest($"Stock insuficiente. Disponible: {existencia.Cantidad}");
            existencia.Cantidad -= mov.Cantidad;
        }

        mov.Fecha = DateTime.Now;
        _db.MovimientosInv.Add(mov);
        await _db.SaveChangesAsync();   // guarda movimiento y existencia juntos

        return CreatedAtAction(nameof(Get), new { id = mov.IdMovimientoInv }, mov);
    }
}