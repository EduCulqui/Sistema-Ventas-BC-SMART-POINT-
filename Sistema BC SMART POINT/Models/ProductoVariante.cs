using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class ProductoVariante
    {
        
        
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int IdProductoVariante { get; set; }

            [Required]
            public int ProductoId { get; set; }

            [Required]
            public int StockActual { get; set; }

            [Required]
            public int StockMinimo { get; set; }

            [Required]
            public bool Estado { get; set; } = true;

            [Required]
            public DateTime FechaRegistro { get; set; }

            public Producto? Producto { get; set; }
        }
    }
