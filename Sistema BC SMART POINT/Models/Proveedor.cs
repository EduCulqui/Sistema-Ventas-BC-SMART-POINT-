using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProveedor { get; set; }

        [Required, StringLength(100)]
        public string NombreEmpresa { get; set; }

        [Required, StringLength(100)]
        public string ContactoNombre { get; set; }

        [Required, StringLength(9)]
        public string Celular { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string Direccion { get; set; }

        [Required, StringLength(11)]
        public string Ruc { get; set; }

        [Required]
        public bool Estado { get; set; } = true;


        // Conexion con Producto
        public ICollection<Producto>? Productos { get; set; }
    }
}
