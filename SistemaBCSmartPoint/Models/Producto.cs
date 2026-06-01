using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(255)]
        public string Descripcion { get; set; }

        [StringLength(200)]
        public string? ImagenUrl { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Capacidad { get; set; }

        [StringLength(50)]
        public string? Modelo { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public int StockActual { get; set; }

        [Required]
        public int StockMinimo { get; set; }

        [Required]
        public bool Estado { get; set; } = true;

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relaciones
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Relación con DetalleVenta 
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }
    }
}

