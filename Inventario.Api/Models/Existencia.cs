using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("Existencias")]
public class Existencia
{
    [Key]
    public int IdExistencia { get; set; }

    public int IdBodega { get; set; }
    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }

    public int IdProducto { get; set; }

    public int Stock { get; set; }

    public int StockMinimo { get; set; }
}