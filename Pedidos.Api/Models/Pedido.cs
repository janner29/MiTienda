using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models;

[Table("Pedidos")]
public class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    public int IdCliente { get; set; }
    [ForeignKey(nameof(IdCliente))]
    public Cliente? Cliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public List<DetallePedido> Detalles { get; set; } = new();
}