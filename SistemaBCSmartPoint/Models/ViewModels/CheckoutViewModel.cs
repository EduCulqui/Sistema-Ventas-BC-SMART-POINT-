using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // Dirección de envío
        [Required] public string DireccionEnvio { get; set; } = string.Empty;
        [Required] public string Ciudad { get; set; } = string.Empty;
        public string? CodigoPostal { get; set; }

        // Pago
        [Required] public string MetodoPago { get; set; } = "Efectivo";

        // Cupón opcional
        public string? CodigoCupon { get; set; }

        public List<CarritoItemViewModel> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public decimal IGV { get; set; }
        public decimal Total { get; set; }
    }
}
