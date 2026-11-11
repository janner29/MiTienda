using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("ReservaStock")]
public class ReservaStock
{
    [Key]
    public int IdReservaStock { get; set; }

    public int IdBodega { get; set; }
    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Activa";   // Activa | Cancelada

    public DateTime Fecha { get; set; } = DateTime.Now;
}