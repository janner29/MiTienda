using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options) { }

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInv> MovimientosInv => Set<MovimientoInv>();
    public DbSet<ReservaStock> ReservasStock => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Existencia>()
            .HasOne(e => e.Bodega).WithMany(b => b.Existencias)
            .HasForeignKey(e => e.IdBodega).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoInv>()
            .HasOne(m => m.Bodega).WithMany(b => b.Movimientos)
            .HasForeignKey(m => m.IdBodega).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReservaStock>()
            .HasOne(r => r.Bodega).WithMany(b => b.Reservas)
            .HasForeignKey(r => r.IdBodega).OnDelete(DeleteBehavior.Restrict);
    }
}