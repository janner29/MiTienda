using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [JsonIgnore]
    public List<Producto> Productos { get; set; } = new();
}