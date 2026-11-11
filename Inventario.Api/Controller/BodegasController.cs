using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController : ControllerBase
{
    private readonly InventarioDbContext _db;
    public BodegasController(InventarioDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bodega>>> Get()
        => await _db.Bodegas.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Bodega>> Get(int id)
    {
        var bodega = await _db.Bodegas.AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdBodega == id);
        return bodega is null ? NotFound() : Ok(bodega);
    }

    [HttpPost]
    public async Task<ActionResult<Bodega>> Post(Bodega bodega)
    {
        _db.Bodegas.Add(bodega);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = bodega.IdBodega }, bodega);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Bodega bodega)
    {
        if (id != bodega.IdBodega) return BadRequest("El ID no coincide");
        _db.Entry(bodega).State = EntityState.Modified;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var bodega = await _db.Bodegas.FindAsync(id);
        if (bodega is null) return NotFound();
        _db.Bodegas.Remove(bodega);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}