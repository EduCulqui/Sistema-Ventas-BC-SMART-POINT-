using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required, StringLength(100)] public string Nombres { get; set; } = string.Empty;
        [Required, StringLength(100)] public string Apellidos { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;

        [Required, Compare("Contrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasenia { get; set; } = string.Empty;

        [Phone, StringLength(9, MinimumLength = 9)]
        public string? Celular { get; set; }

        [Required, StringLength(200)] public string Direccion { get; set; } = string.Empty;
        [Required, StringLength(80)] public string Ciudad { get; set; } = string.Empty;
        [Required, StringLength(12)] public string CodigoPostal { get; set; } = string.Empty;
        [Required, StringLength(11, MinimumLength = 8)] public string DniRuc { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
    }
}
