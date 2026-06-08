using System.ComponentModel.DataAnnotations;

namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // Dirección de envío
        [Required, StringLength(200)] public string DireccionEnvio { get; set; } = string.Empty;
        [Required, StringLength(80)] public string Ciudad { get; set; } = string.Empty;
        [StringLength(12)] public string? CodigoPostal { get; set; }

        // Pago
        [Required, StringLength(30)] public string MetodoPago { get; set; } = "Efectivo";

        // Cupón opcional
        [StringLength(12)] public string? CodigoCupon { get; set; }

        public List<CarritoItemViewModel> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public decimal IGV { get; set; }
        public decimal Total { get; set; }
    }
}
