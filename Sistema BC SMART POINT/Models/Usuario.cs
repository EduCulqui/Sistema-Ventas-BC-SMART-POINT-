using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required, StringLength(100)]
        public string Nombres { get; set; }

        [Required, StringLength(100)]
        public string Apellidos { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string Contrasenia { get; set; }

        [Required, StringLength(50)]
        public string Rol { get; set; }

        [Required, StringLength(9)]
        public string Celular { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public bool Estado { get; set; } = true;


        // Conexion con Cliente (relación 1:0..1)
        public Cliente? Cliente { get; set; }

        // Conexion con Administrador (relación 1:0..1)
        public Administrador? Administrador { get; set; }
    }
}
