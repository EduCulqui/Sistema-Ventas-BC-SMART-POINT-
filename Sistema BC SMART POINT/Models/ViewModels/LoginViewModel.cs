using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;

        public bool Recordarme { get; set; }

        // URL a la que regresar después del login
        public string? ReturnUrl { get; set; }
    }
}
