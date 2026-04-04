using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Envio
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEnvio { get; set; }

        [Required, StringLength(100)]
        public string DireccionEnvio { get; set; }

        [Required, StringLength(50)]
        public string Ciudad { get; set; }

        [Required, StringLength(6)]
        public string CodigoPostal { get; set; }

        [Required]
        public DateTime FechaEnvio { get; set; }

        [Required]
        public DateTime FechaEntregaEstimada { get; set; }

        public DateTime? FechaEntregaReal { get; set; }

        //(pendiente, en proceso, alistado, enviado, entregado, cancelado)
        [Required, StringLength(20)]
        public string EstadoEnvio { get; set; }

        [Required, StringLength(50)]
        public string EmpresaTransporte { get; set; }

        [Required]
        public string NumeroSeguimiento { get; set; }


    }
}
