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

        [Required]
        public decimal PrecioCompra { get; set; }

        [Required]
        public decimal PrecioVenta { get; set; }

        [Required, StringLength(200)]
        public string ImagenUrl { get; set; }

        [Required]
        public bool Estado { get; set; } = true;

        [Required]
        public DateTime FechaRegistro { get; set; }

        // Conexion con Categoria
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Conexion con Proveedor
        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Conexion con DetalleVenta
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }

        // Conexion con ProductoVariante (stock se maneja por variante)
        public ICollection<ProductoVariante>? Variantes { get; set; }

        // Propiedad calculada: Stock total de todas las variantes activas
        [NotMapped]
        public int StockTotal => Variantes?.Where(v => v.Estado).Sum(v => v.StockActual) ?? 0;

    }
}

