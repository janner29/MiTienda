using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public ExistenciasController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

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
            .Where(e => e.Stock < e.StockMinimo)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Existencia>> Post(Existencia existencia)
    {
        var error = await Validar(existencia);
        if (error is not null) return error;

        if (await _db.Existencias.AnyAsync(e =>
                e.IdBodega == existencia.IdBodega && e.IdProducto == existencia.IdProducto))
            return Conflict("Ya existe ese producto en esa bodega");

        existencia.IdExistencia = 0;
        _db.Existencias.Add(existencia);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = existencia.IdExistencia }, existencia);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Existencia existencia)
    {
        if (id != existencia.IdExistencia) return BadRequest("El ID no coincide");
        if (!await _db.Existencias.AnyAsync(e => e.IdExistencia == id)) return NotFound();

        var error = await Validar(existencia);
        if (error is not null) return error;

        _db.Entry(existencia).State = EntityState.Modified;
        await _db.SaveChangesAsync();
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

    // Devuelve null si todo está bien, o la respuesta de error (400 / 503)
    private async Task<ActionResult?> Validar(Existencia e)
    {
        if (e.Stock < 0 || e.StockMinimo < 0)
            return BadRequest("Stock y StockMinimo no pueden ser negativos");

        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == e.IdBodega))
            return BadRequest("La bodega no existe");

        try
        {
            if (!await _catalogo.ProductoExisteAsync(e.IdProducto))
                return BadRequest("El producto no existe en Catalogo");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Catalogo.Api no está disponible");
        }

        return null;
    }
}