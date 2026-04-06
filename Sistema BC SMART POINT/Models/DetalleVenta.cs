using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetalleVenta { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public decimal PrecioUnitario { get; set; }
        [Required]
        public decimal SubTotal { get; set; }

        // Conexion con Venta
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        // Conexion con Producto
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

    }
}
