using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;
using System.Security.Claims;

namespace Sistema_BC_SMART_POINT.Controllers
    {
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly CarritoService _carrito;
        private readonly VentaService _venta;
        private readonly AppDbContext _db;
        private readonly ConfiguracionPagoViewModel _configPago;

        public CarritoController(CarritoService carrito, VentaService venta,
            AppDbContext db, IOptions<ConfiguracionPagoViewModel> configPago)
        {
            _carrito = carrito;
            _venta = venta;
            _db = db;
            _configPago = configPago.Value;
        }

        // GET /Carrito
        public IActionResult Index()
        {
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            return View(items);
        }

        // POST /Carrito/Agregar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Detalle", "Catalogo", new { id = productoId });
            }

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
            if (!ModelState.IsValid) return RedirectToAction("Index");
            _carrito.QuitarItem(HttpContext.Session, productoId);
            return RedirectToAction("Index");
        }

        // GET /Carrito/Checkout — Paso 1: datos + método
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

        // POST /Carrito/ProcederPago — decide a dónde ir según método
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcederPago(CheckoutViewModel vm)
        {
            // Reconstruir items desde session
            vm.Items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Subtotal = vm.Items.Sum(i => i.SubTotal);

            // Aplicar descuento si hay cupón
            if (!string.IsNullOrEmpty(vm.CodigoCupon))
            {
                var (valido, pct, _) = await _venta.ValidarCuponAsync(vm.CodigoCupon);
                vm.DescuentoAplicado = valido ? pct : 0;
                if (!valido)
                    ModelState.AddModelError("CodigoCupon", "Cupón inválido o vencido.");
            }

            decimal descMonto = vm.Subtotal * (vm.DescuentoAplicado / 100m);
            decimal baseCalc = vm.Subtotal - descMonto;
            vm.IGV = Math.Round(baseCalc * 0.18m, 2);
            vm.Total = baseCalc + vm.IGV;

            if (!ModelState.IsValid) return View("Checkout", vm);

            // Guardar vm en TempData para el siguiente paso
            TempData["CheckoutDireccion"] = vm.DireccionEnvio;
            TempData["CheckoutCiudad"] = vm.Ciudad;
            TempData["CheckoutPostal"] = vm.CodigoPostal;
            TempData["CheckoutMetodo"] = vm.MetodoPago;
            TempData["CheckoutCupon"] = vm.CodigoCupon;
            TempData["CheckoutDescuento"] = vm.DescuentoAplicado.ToString();

            var metodosConPago = new[] { "Yape", "Plin", "Transferencia" };

            // Si requiere comprobante → ir a PagoTransferencia
            if (metodosConPago.Contains(vm.MetodoPago))
                return RedirectToAction("PagoTransferencia");

            // Si es efectivo o tarjeta → confirmar directo
            return RedirectToAction("ConfirmarDirecto");
        }

        // GET /Carrito/PagoTransferencia — Paso 2: QR + subir comprobante
        public IActionResult PagoTransferencia()
        {
            // Recuperar datos del checkout
            var metodo = TempData.Peek("CheckoutMetodo")?.ToString();
            if (string.IsNullOrEmpty(metodo)) return RedirectToAction("Checkout");

            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            decimal sub = items.Sum(i => i.SubTotal);
            decimal desc = decimal.TryParse(TempData.Peek("CheckoutDescuento")?.ToString(),
                            out var d) ? d : 0;
            decimal base2 = sub - (sub * desc / 100m);
            decimal igv = Math.Round(base2 * 0.18m, 2);
            decimal total = base2 + igv;

            ViewBag.Metodo = metodo;
            ViewBag.Total = total;
            ViewBag.ConfigPago = _configPago;

            return View();
        }

        // POST /Carrito/ConfirmarConComprobante
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarConComprobante(IFormFile comprobante)
        {
            if (!ModelState.IsValid) return RedirectToAction("PagoTransferencia");

            // Validar archivo
            if (comprobante == null || comprobante.Length == 0)
            {
                TempData["ErrorComp"] = "Debes subir la captura del pago.";
                return RedirectToAction("PagoTransferencia");
            }

            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(comprobante.FileName).ToLower();
            if (!extensiones.Contains(ext))
            {
                TempData["ErrorComp"] = "Solo se permiten imágenes JPG, PNG o WEBP.";
                return RedirectToAction("PagoTransferencia");
            }

            if (comprobante.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorComp"] = "La imagen no debe superar 5MB.";
                return RedirectToAction("PagoTransferencia");
            }

            // Obtener cliente
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return Unauthorized();

            // Reconstruir CheckoutViewModel desde TempData
            var vm = new CheckoutViewModel
            {
                DireccionEnvio = TempData["CheckoutDireccion"]?.ToString() ?? "",
                Ciudad = TempData["CheckoutCiudad"]?.ToString() ?? "",
                CodigoPostal = TempData["CheckoutPostal"]?.ToString(),
                MetodoPago = TempData["CheckoutMetodo"]?.ToString() ?? "",
                CodigoCupon = TempData["CheckoutCupon"]?.ToString(),
                DescuentoAplicado = decimal.TryParse(
                    TempData["CheckoutDescuento"]?.ToString(), out var dp) ? dp : 0
            };

            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Items = items;

            // Registrar venta
            int idVenta = await _venta.RegistrarVentaAsync(cliente.IdCliente, vm, items);

            // Guardar comprobante
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(),
                            "wwwroot", "comprobantes");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"venta_{idVenta}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                await comprobante.CopyToAsync(stream);

            // Actualizar venta con comprobante
            var venta = await _db.Ventas.FindAsync(idVenta);
            if (venta != null)
            {
                venta.ComprobantePago = $"/comprobantes/{nombreArchivo}";
                venta.FechaComprobante = DateTime.Now;
                venta.EstadoPago = "En verificación";
                await _db.SaveChangesAsync();
            }

            _carrito.Limpiar(HttpContext.Session);

            return RedirectToAction("Confirmacion", new { idVenta });
        }

        // POST /Carrito/ConfirmarDirecto — para Efectivo/Tarjeta
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarDirecto()
        {
            if (!ModelState.IsValid) return RedirectToAction("Checkout");

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return Unauthorized();

            var vm = new CheckoutViewModel
            {
                DireccionEnvio = TempData["CheckoutDireccion"]?.ToString() ?? "",
                Ciudad = TempData["CheckoutCiudad"]?.ToString() ?? "",
                CodigoPostal = TempData["CheckoutPostal"]?.ToString(),
                MetodoPago = TempData["CheckoutMetodo"]?.ToString() ?? "",
                CodigoCupon = TempData["CheckoutCupon"]?.ToString(),
                DescuentoAplicado = decimal.TryParse(
                    TempData["CheckoutDescuento"]?.ToString(), out var dp) ? dp : 0
            };

            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Items = items;

            int idVenta = await _venta.RegistrarVentaAsync(cliente.IdCliente, vm, items);
            _carrito.Limpiar(HttpContext.Session);

            return RedirectToAction("Confirmacion", new { idVenta });
        }

        // GET /Carrito/Confirmacion
        public async Task<IActionResult> Confirmacion(int idVenta)
        {
            var venta = await _db.Ventas
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null) return NotFound();
            return View(venta);
        }

        // GET /Carrito/MisPedidos
        public async Task<IActionResult> MisPedidos()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return RedirectToAction("Index", "Catalogo");

            var pedidos = await _db.Ventas
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .Include(v => v.CuponDescuento)
                .Where(v => v.ClienteId == cliente.IdCliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return View(pedidos);
        }

        // GET /Carrito/DetallePedido
        public async Task<IActionResult> DetallePedido(int idVenta)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return Unauthorized();

            var venta = await _db.Ventas
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .Include(v => v.CuponDescuento)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta
                                        && v.ClienteId == cliente.IdCliente);

            if (venta == null) return NotFound();
            return View(venta);
        }
    }
}
