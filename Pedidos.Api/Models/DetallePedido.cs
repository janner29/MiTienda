using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("DetallePedidos")]
public class DetallePedido
{
    [Key]
    public int IdDetalle { get; set; }

    public int IdPedido { get; set; }
    [JsonIgnore]
    [ForeignKey(nameof(IdPedido))]
    public Pedido? Pedido { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
}