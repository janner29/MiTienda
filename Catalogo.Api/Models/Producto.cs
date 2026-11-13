using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Activo";   // Activo | Inactivo

    public int IdMarca { get; set; }
    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }

    public int IdCategoria { get; set; }
    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }
}