using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProveedor { get; set; }

        [Required, StringLength(100)]
        public string NombreEmpresa { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string ContactoNombre { get; set; } = string.Empty;

        [Required, Phone, StringLength(9, MinimumLength = 9)]
        public string Celular { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required, StringLength(11, MinimumLength = 11)]
        public string Ruc { get; set; } = string.Empty;

        [Required]
        public bool Estado { get; set; } = true;


        // Conexion con Producto
        public ICollection<Producto>? Productos { get; set; }
    }
}
