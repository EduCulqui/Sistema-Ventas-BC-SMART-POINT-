
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Cliente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCliente { get; set; }
        [Required, StringLength (200)]
        public string Direccion { get; set; }
        [Required, StringLength (200)]
        public string Ciudad { get; set; }
        [Required, StringLength (200)]
        public string CodigoPostal { get; set; }
        [Required, StringLength (200)]
        public string DniRuc { get; set; }
        [Required]
        public DateTime FechaNacimiento { get; set; }

        // Conexion con Usuario
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // Conexion con Venta
        public ICollection<Venta>? Ventas { get; set; }
    }
}
