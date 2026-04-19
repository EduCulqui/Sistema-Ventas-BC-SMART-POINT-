using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasenia { get; set; } = string.Empty;

        public bool Recordarme { get; set; }

        // URL a la que regresar después del login (ej. /Carrito)
        public string? ReturnUrl { get; set; }
    }
}
