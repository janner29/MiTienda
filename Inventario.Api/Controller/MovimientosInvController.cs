using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInvController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public MovimientosInvController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

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
            .FirstOrDefaultAsync(m => m.IdMovimiento == id);
        return mov is null ? NotFound() : Ok(mov);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoInv>> Post(MovimientoInv mov)
    {
        if (mov.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0");

        if (mov.Tipo != "Entrada" && mov.Tipo != "Salida")
            return BadRequest("Tipo debe ser 'Entrada' o 'Salida'");

        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == mov.IdBodega))
            return BadRequest("La bodega no existe");

        try
        {
            if (!await _catalogo.ProductoExisteAsync(mov.IdProducto))
                return BadRequest("El producto no existe en Catalogo");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Catalogo.Api no está disponible");
        }

        var existencia = await _db.Existencias.FirstOrDefaultAsync(e =>
            e.IdBodega == mov.IdBodega && e.IdProducto == mov.IdProducto);

        if (existencia is null)
        {
            if (mov.Tipo == "Salida")
                return BadRequest("No hay existencias de ese producto en la bodega");

            existencia = new Existencia
            {
                IdBodega = mov.IdBodega,
                IdProducto = mov.IdProducto,
                Stock = 0,
                StockMinimo = 0
            };
            _db.Existencias.Add(existencia);
        }

        if (mov.Tipo == "Entrada")
        {
            existencia.Stock += mov.Cantidad;
        }
        else
        {
            if (existencia.Stock < mov.Cantidad)
                return BadRequest($"Stock insuficiente. Disponible: {existencia.Stock}");
            existencia.Stock -= mov.Cantidad;
        }

        mov.IdMovimiento = 0;
        mov.Fecha = DateTime.Now;
        _db.MovimientosInv.Add(mov);
        await _db.SaveChangesAsync();   // guarda movimiento y existencia juntos

        return CreatedAtAction(nameof(Get), new { id = mov.IdMovimiento }, mov);
    }
}