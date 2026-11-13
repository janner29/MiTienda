using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoDbContext _db;
    public ProductosController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> Get()
        => await _db.Productos
            .AsNoTracking()
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> Get(int id)
    {
        var producto = await _db.Productos
            .AsNoTracking()
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Post(Producto producto)
    {
        var error = await Validar(producto);
        if (error is not null) return error;

        producto.IdProducto = 0;
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = producto.IdProducto }, producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Producto producto)
    {
        if (id != producto.IdProducto) return BadRequest("El ID no coincide");
        if (!await _db.Productos.AnyAsync(p => p.IdProducto == id)) return NotFound();

        var error = await Validar(producto);
        if (error is not null) return error;

        _db.Entry(producto).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Devuelve null si todo está bien, o la respuesta de error (400 / 409)
    private async Task<ActionResult?> Validar(Producto p)
    {
        if (p.Precio <= 0)
            return BadRequest("El precio debe ser mayor a 0");

        if (p.Estado != "Activo" && p.Estado != "Inactivo")
            return BadRequest("Estado debe ser 'Activo' o 'Inactivo'");

        if (!await _db.Marcas.AnyAsync(m => m.IdMarca == p.IdMarca))
            return BadRequest("La marca no existe");

        if (!await _db.Categorias.AnyAsync(c => c.IdCategoria == p.IdCategoria))
            return BadRequest("La categoría no existe");

        if (await _db.Productos.AnyAsync(x => x.SKU == p.SKU && x.IdProducto != p.IdProducto))
            return Conflict("Ya existe un producto con ese SKU");

        return null;
    }
}