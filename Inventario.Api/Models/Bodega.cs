using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventario.Api.Models;

[Table("Bodega")]
public class Bodega
{
    [Key]
    public int IdBodega { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Ubicacion { get; set; } = string.Empty;

    [JsonIgnore]
    public List<Existencia> Existencias { get; set; } = new();
    [JsonIgnore]
    public List<MovimientoInv> Movimientos { get; set; } = new();
    [JsonIgnore]
    public List<ReservaStock> Reservas { get; set; } = new();
}