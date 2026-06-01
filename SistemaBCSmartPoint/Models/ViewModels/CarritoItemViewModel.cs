namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string? Color { get; set; }
        public string? Capacidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal => PrecioUnitario * Cantidad;
    }
}
