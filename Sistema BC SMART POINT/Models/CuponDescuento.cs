using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class CuponDescuento
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCuponDescuento {  get; set; }
        [Required, StringLength(12, MinimumLength = 3)]
        public string CodigoCupon {  get; set; } = string.Empty;
        [Required, Range(1, 95)]
        public decimal PorcentajeDescuento { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }
        [Required]
        public bool Estado {  get; set; }
        [Required]
        public DateTime FechaCreación {  get; set; }

        // Conexion con Venta
        public ICollection<Venta>? Ventas { get; set; }

    }
}
