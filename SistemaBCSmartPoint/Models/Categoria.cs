using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_BC_SMART_POINT.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCategoria { get; set; }
        [Required, StringLength(50)]
        public string NomCategoria { get; set; }
        [Required, StringLength(500)]
        public string Descripcion { get; set; }
        [Required]
        public bool Estado { get; set; }

        // Conexion con Producto
        public ICollection<Producto>? Productos { get; set; }
    }
}
