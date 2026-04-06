using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class AlertaStock
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAlertaStock { get; set; }
        [Required]
        public DateTime FechaAlerta { get; set; }
        [Required, StringLength(20)]
        public string Descripcion { get; set; }
        [Required, StringLength(255)]
        public bool Estado { get; set; }

        public DateTime FechaResolucion { get; set; }

        // Conexion con ProductoVariante (en lugar de Producto)
        [Required]
        public int ProductoVarianteId { get; set; }
        public ProductoVariante? ProductoVariante { get; set; }
    }
}
