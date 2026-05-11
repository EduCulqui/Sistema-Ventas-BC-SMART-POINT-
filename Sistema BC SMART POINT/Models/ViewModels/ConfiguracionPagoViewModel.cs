namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class ConfiguracionPagoViewModel
    {
        public string NombreTitular { get; set; } = string.Empty;
        public string? NumeroYape { get; set; }
        public string? NumeroPlin { get; set; }
        public string? NumeroCuenta { get; set; }
        public string? BancoCuenta { get; set; }
        public string? QrImagenUrl { get; set; }
    }
}
