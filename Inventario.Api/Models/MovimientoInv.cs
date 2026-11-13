using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("MovimientosInv")]
public class MovimientoInv
{
    [Key]
    public int IdMovimiento { get; set; }

    public int IdBodega { get; set; }
    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }

    public int IdProducto { get; set; }

    [Required]
    [StringLength(10)]
    public string Tipo { get; set; } = string.Empty;   // Entrada | Salida

    public int Cantidad { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;
}