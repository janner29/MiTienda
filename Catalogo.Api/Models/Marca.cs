using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? PaisOrigen { get; set; }

    [JsonIgnore]
    public List<Producto> Productos { get; set; } = new();
}