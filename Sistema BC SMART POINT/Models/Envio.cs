using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Envio
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEnvio { get; set; }

        [Required, StringLength(200)]
        public string DireccionEnvio { get; set; }

        [Required, StringLength(100)]
        public string Ciudad { get; set; }

        [StringLength(10)]
        public string? CodigoPostal { get; set; }

        // Se llena cuando el admin despacha
        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }

        // pendiente → alistado → despachado → en camino → entregado → devuelto
        [Required, StringLength(30)]
        public string EstadoEnvio { get; set; } = "Preparando";

        [StringLength(100)]
        public string? EmpresaTransporte { get; set; }

        [StringLength(100)]
        public string? NumeroSeguimiento { get; set; }

        // Conexion con Venta
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        // Conexion con Administrador
        public int? AdministradorId { get; set; }
        public Administrador? Administrador { get; set; }


    }
}
