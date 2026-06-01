using Sistema_BC_SMART_POINT.Models.ViewModels;
using System.Text.Json;

namespace Sistema_BC_SMART_POINT.Services
{
    public class CarritoService
    {
        private const string SessionKey = "Carrito_BcSmartPoint";

        // Leer el carrito desde Session
        public List<CarritoItemViewModel> ObtenerCarrito(ISession session)
        {
            var json = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CarritoItemViewModel>();
            return JsonSerializer.Deserialize<List<CarritoItemViewModel>>(json)
                   ?? new List<CarritoItemViewModel>();
        }

        // Guardar el carrito en Session
        private void GuardarCarrito(ISession session, List<CarritoItemViewModel> carrito)
        {
            session.SetString(SessionKey, JsonSerializer.Serialize(carrito));
        }

        // Agregar o incrementar cantidad
        public void AgregarItem(ISession session, CarritoItemViewModel item)
        {
            var carrito = ObtenerCarrito(session);
            var existente = carrito.FirstOrDefault(c => c.ProductoId == item.ProductoId);
            if (existente != null)
                existente.Cantidad += item.Cantidad;
            else
                carrito.Add(item);
            GuardarCarrito(session, carrito);
        }

        // Quitar un ítem por ProductoId
        public void QuitarItem(ISession session, int productoId)
        {
            var carrito = ObtenerCarrito(session);
            carrito.RemoveAll(c => c.ProductoId == productoId);
            GuardarCarrito(session, carrito);
        }

        // Vaciar carrito completo
        public void Limpiar(ISession session) => session.Remove(SessionKey);

        public int ContarItems(ISession session) =>
            ObtenerCarrito(session).Sum(c => c.Cantidad);
    }
}
