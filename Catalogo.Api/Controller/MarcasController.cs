using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _db;
    public MarcasController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marca>>> Get()
        => await _db.Marcas.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Marca>> Get(int id)
    {
        var marca = await _db.Marcas.AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMarca == id);
        return marca is null ? NotFound() : Ok(marca);
    }

    [HttpPost]
    public async Task<ActionResult<Marca>> Post(Marca marca)
    {
        _db.Marcas.Add(marca);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Marca marca)
    {
        if (id != marca.IdMarca) return BadRequest("El ID no coincide");
        _db.Entry(marca).State = EntityState.Modified;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Marcas.AnyAsync(m => m.IdMarca == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var marca = await _db.Marcas.FindAsync(id);
        if (marca is null) return NotFound();
        _db.Marcas.Remove(marca);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}