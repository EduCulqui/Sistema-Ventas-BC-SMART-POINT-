namespace Sistema_BC_SMART_POINT.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Producto> ProductosFiltrados { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public int? CategoriaActual { get; set; }
        public string BusquedaActual { get; set; } = "";
        public string OrdenActual { get; set; } = "";
    }
}
