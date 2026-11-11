using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _db;
    public ExistenciasController(InventarioDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Existencia>>> Get()
        => await _db.Existencias.AsNoTracking().Include(e => e.Bodega).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Existencia>> Get(int id)
    {
        var existencia = await _db.Existencias.AsNoTracking()
            .Include(e => e.Bodega)
            .FirstOrDefaultAsync(e => e.IdExistencia == id);
        return existencia is null ? NotFound() : Ok(existencia);
    }

    // GET: api/Existencias/producto/1  -> existencias de un producto en todas las bodegas
    [HttpGet("producto/{idProducto}")]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetPorProducto(int idProducto)
        => await _db.Existencias.AsNoTracking()
            .Include(e => e.Bodega)
            .Where(e => e.IdProducto == idProducto)
            .ToListAsync();

    // GET: api/Existencias/bajo-stock
    [HttpGet("bajo-stock")]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetBajoStock()
        => await _db.Existencias.AsNoTracking()
            .Include(e => e.Bodega)
            .Where(e => e.Cantidad < e.StockMinimo)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Existencia>> Post(Existencia existencia)
    {
        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == existencia.IdBodega))
            return BadRequest("La bodega no existe");

        if (await _db.Existencias.AnyAsync(e =>
                e.IdBodega == existencia.IdBodega && e.IdProducto == existencia.IdProducto))
            return Conflict("Ya existe ese producto en esa bodega");

        _db.Existencias.Add(existencia);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = existencia.IdExistencia }, existencia);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Existencia existencia)
    {
        if (id != existencia.IdExistencia) return BadRequest("El ID no coincide");
        _db.Entry(existencia).State = EntityState.Modified;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Existencias.AnyAsync(e => e.IdExistencia == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existencia = await _db.Existencias.FindAsync(id);
        if (existencia is null) return NotFound();
        _db.Existencias.Remove(existencia);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}