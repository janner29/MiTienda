using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("Cliente")]
public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required]
    [StringLength(20)]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Nombres { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Correo { get; set; }

    [JsonIgnore]
    public List<Pedido> Pedidos { get; set; } = new();
}