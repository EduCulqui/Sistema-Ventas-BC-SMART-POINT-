using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Sistema_BC_SMART_POINT.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        
        private readonly CarritoService _carrito;
        private readonly VentaService _venta;
        private readonly AppDbContext _db;

        public CarritoController(CarritoService carrito, VentaService venta, AppDbContext db)
        {
            _carrito = carrito;
            _venta = venta;
            _db = db;
        }

        // GET /Carrito
        public IActionResult Index()
        {
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            return View(items);
        }

        // POST /Carrito/Agregar  
        //se llama desde el detalle del producto
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            var prod = await _db.Productos.FindAsync(productoId);
            if (prod == null || prod.StockActual < cantidad)
            {
                TempData["Error"] = "Producto no disponible o sin stock suficiente.";
                return RedirectToAction("Detalle", "Catalogo", new { id = productoId });
            }

            _carrito.AgregarItem(HttpContext.Session, new CarritoItemViewModel
            {
                ProductoId = prod.IdProducto,
                Nombre = prod.Nombre,
                ImagenUrl = prod.ImagenUrl,
                Color = prod.Color,
                Capacidad = prod.Capacidad,
                PrecioUnitario = prod.Precio,
                Cantidad = cantidad
            });

            TempData["Exito"] = $"{prod.Nombre} agregado al carrito.";
            return RedirectToAction("Index");
        }

        // POST /Carrito/Quitar
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Quitar(int productoId)
        {
            _carrito.QuitarItem(HttpContext.Session, productoId);
            return RedirectToAction("Index");
        }

        // GET /Carrito/Checkout
        public IActionResult Checkout()
        {
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            if (!items.Any()) return RedirectToAction("Index");

            var vm = new CheckoutViewModel
            {
                Items = items,
                Subtotal = items.Sum(i => i.SubTotal)
            };
            vm.IGV = Math.Round(vm.Subtotal * 0.18m, 2);
            vm.Total = vm.Subtotal + vm.IGV;

            return View(vm);
        }

        // POST /Carrito/AplicarCupon  
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AplicarCupon(string codigo)
        {
            var (valido, pct, _) = await _venta.ValidarCuponAsync(codigo);
            if (!valido)
            {
                TempData["ErrorCupon"] = "Cupón inválido o vencido.";
                return RedirectToAction("Checkout");
            }
            TempData["DescuentoPct"] = pct.ToString();
            TempData["CodigoCupon"] = codigo;
            TempData["ExitoCupon"] = $"Cupón aplicado: {pct}% de descuento.";
            return RedirectToAction("Checkout");
        }

        // POST /Carrito/ConfirmarPedido
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPedido(CheckoutViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Items = _carrito.ObtenerCarrito(HttpContext.Session);
                return View("Checkout", vm);
            }

            // Obtener IdCliente desde el claim del usuario logueado
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return Unauthorized();

            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            int idVenta = await _venta.RegistrarVentaAsync(cliente.IdCliente, vm, items);

            _carrito.Limpiar(HttpContext.Session);

            TempData["Exito"] = $"Pedido #{idVenta} registrado correctamente.";
            return RedirectToAction("Confirmacion", new { idVenta });
        }

        // GET /Carrito/Confirmacion/12
        public async Task<IActionResult> Confirmacion(int idVenta)
        {
            var venta = await _db.Ventas
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null) return NotFound();
            return View(venta);
        }
    }
}
