
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
            
        public Cliente() { }
    }
}
