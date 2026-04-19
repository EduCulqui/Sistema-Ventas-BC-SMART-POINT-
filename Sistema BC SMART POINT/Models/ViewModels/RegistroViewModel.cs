using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required] public string Nombres { get; set; } = string.Empty;
        [Required] public string Apellidos { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;

        [Compare("Contrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasenia { get; set; } = string.Empty;

        public string? Celular { get; set; }

        [Required] public string Direccion { get; set; } = string.Empty;
        [Required] public string Ciudad { get; set; } = string.Empty;
        [Required] public string CodigoPostal { get; set; } = string.Empty;
        [Required] public string DniRuc { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
    }
}
