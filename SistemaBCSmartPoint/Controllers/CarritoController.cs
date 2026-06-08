using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
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

        // Constantes de acciones
        private const string AccionIndex = "Index";
        private const string AccionPagoTransferencia = "PagoTransferencia";
        private const string AccionMisPedidos = "MisPedidos";
        private const string AccionConfirmacion = "Confirmacion";
        private const string AccionCheckout = "Checkout";

        // Constantes de TempData
        private const string KeyCheckoutDireccion = "CheckoutDireccion";
        private const string KeyCheckoutCiudad = "CheckoutCiudad";
        private const string KeyCheckoutPostal = "CheckoutPostal";
        private const string KeyCheckoutMetodo = "CheckoutMetodo";
        private const string KeyCheckoutCupon = "CheckoutCupon";
        private const string KeyCheckoutDescuento = "CheckoutDescuento";

        public CarritoController(CarritoService carrito, VentaService venta,
            AppDbContext db, IOptions<ConfiguracionPagoViewModel> configPago)
        {
            _carrito = carrito;
            _venta = venta;
            _db = db;
            _configPago = configPago.Value;
        }

        // ─────────────────────────────────────────────
        // Métodos auxiliares privados (eliminan duplicación)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Reconstruye el CheckoutViewModel desde TempData.
        /// Centraliza el bloque duplicado en ConfirmarConComprobante y ConfirmarDirecto.
        /// </summary>
        private CheckoutViewModel ObtenerVmDesdeTempData()
        {
            return new CheckoutViewModel
            {
                DireccionEnvio = TempData[KeyCheckoutDireccion]?.ToString() ?? "",
                Ciudad = TempData[KeyCheckoutCiudad]?.ToString() ?? "",
                CodigoPostal = TempData[KeyCheckoutPostal]?.ToString(),
                MetodoPago = TempData[KeyCheckoutMetodo]?.ToString() ?? "",
                CodigoCupon = TempData[KeyCheckoutCupon]?.ToString(),
                DescuentoAplicado = decimal.TryParse(
                    TempData[KeyCheckoutDescuento]?.ToString(), out var dp) ? dp : 0
            };
        }

        /// <summary>
        /// Calcula IGV y Total a partir del subtotal y el porcentaje de descuento.
        /// Centraliza el cálculo duplicado en Checkout, ProcederPago y PagoTransferencia.
        /// </summary>
        private static (decimal igv, decimal total) CalcularTotales(decimal subtotal, decimal descuentoPct)
        {
            decimal descMonto = subtotal * (descuentoPct / 100m);
            decimal baseCalc = subtotal - descMonto;
            decimal igv = Math.Round(baseCalc * 0.18m, 2);
            return (igv, baseCalc + igv);
        }

        /// <summary>
        /// Obtiene el cliente autenticado a partir del claim del usuario.
        /// Centraliza el bloque duplicado en ConfirmarConComprobante, ConfirmarDirecto,
        /// MisPedidos y DetallePedido.
        /// </summary>
        private async Task<(int usuarioId, Cliente? cliente)> ObtenerClienteAsync()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            return (usuarioId, cliente);
        }

        // ─────────────────────────────────────────────
        // Acciones públicas
        // ─────────────────────────────────────────────

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
                return RedirectToAction(AccionIndex, "Catalogo");

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
            return RedirectToAction(AccionIndex);
        }

        // POST /Carrito/Quitar
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Quitar(int productoId)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(AccionIndex);

            _carrito.QuitarItem(HttpContext.Session, productoId);
            return RedirectToAction(AccionIndex);
        }

        // GET /Carrito/Checkout — Paso 1: datos + método
        public IActionResult Checkout()
        {
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            if (!items.Any()) return RedirectToAction(AccionIndex);

            decimal subtotal = items.Sum(i => i.SubTotal);
            var (igv, total) = CalcularTotales(subtotal, 0);

            var vm = new CheckoutViewModel
            {
                Items = items,
                Subtotal = subtotal,
                IGV = igv,
                Total = total
            };

            return View(vm);
        }

        // POST /Carrito/ProcederPago — decide a dónde ir según método
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcederPago(CheckoutViewModel vm)
        {
            vm.Items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Subtotal = vm.Items.Sum(i => i.SubTotal);

            if (!string.IsNullOrEmpty(vm.CodigoCupon))
            {
                var (valido, pct, _) = await _venta.ValidarCuponAsync(vm.CodigoCupon);
                vm.DescuentoAplicado = valido ? pct : 0;
                if (!valido)
                    ModelState.AddModelError("CodigoCupon", "Cupón inválido o vencido.");
            }

            var (igv, total) = CalcularTotales(vm.Subtotal, vm.DescuentoAplicado);
            vm.IGV = igv;
            vm.Total = total;

            if (!ModelState.IsValid) return View(AccionCheckout, vm);

            TempData[KeyCheckoutDireccion] = vm.DireccionEnvio;
            TempData[KeyCheckoutCiudad] = vm.Ciudad;
            TempData[KeyCheckoutPostal] = vm.CodigoPostal;
            TempData[KeyCheckoutMetodo] = vm.MetodoPago;
            TempData[KeyCheckoutCupon] = vm.CodigoCupon;
            TempData[KeyCheckoutDescuento] = vm.DescuentoAplicado.ToString();

            var metodosConPago = new[] { "Yape", "Plin", "Transferencia" };

            if (metodosConPago.Contains(vm.MetodoPago))
                return RedirectToAction(AccionPagoTransferencia);

            return RedirectToAction("ConfirmarDirecto");
        }

        // GET /Carrito/PagoTransferencia — Paso 2: QR + subir comprobante
        public IActionResult PagoTransferencia()
        {
            var metodo = TempData.Peek(KeyCheckoutMetodo)?.ToString();
            if (string.IsNullOrEmpty(metodo)) return RedirectToAction(AccionCheckout);

            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            decimal sub = items.Sum(i => i.SubTotal);
            decimal desc = decimal.TryParse(
                TempData.Peek(KeyCheckoutDescuento)?.ToString(), out var d) ? d : 0;

            var (igv, total) = CalcularTotales(sub, desc);

            ViewBag.Metodo = metodo;
            ViewBag.Total = total;
            ViewBag.ConfigPago = _configPago;

            return View();
        }

        // POST /Carrito/ConfirmarConComprobante
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarConComprobante(IFormFile comprobante)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(AccionPagoTransferencia);

            if (comprobante == null || comprobante.Length == 0)
            {
                TempData["ErrorComp"] = "Debes subir la captura del pago.";
                return RedirectToAction(AccionPagoTransferencia);
            }

            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(comprobante.FileName).ToLower();
            if (!extensiones.Contains(ext))
            {
                TempData["ErrorComp"] = "Solo se permiten imágenes JPG, PNG o WEBP.";
                return RedirectToAction(AccionPagoTransferencia);
            }

            if (comprobante.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorComp"] = "La imagen no debe superar 5MB.";
                return RedirectToAction(AccionPagoTransferencia);
            }

            var (_, cliente) = await ObtenerClienteAsync();
            if (cliente == null) return Unauthorized();

            var vm = ObtenerVmDesdeTempData();
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Items = items;

            int idVenta = await _venta.RegistrarVentaAsync(cliente.IdCliente, vm, items);

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"venta_{idVenta}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                await comprobante.CopyToAsync(stream);

            var venta = await _db.Ventas.FindAsync(idVenta);
            if (venta != null)
            {
                venta.ComprobantePago = $"/comprobantes/{nombreArchivo}";
                venta.FechaComprobante = DateTime.Now;
                venta.EstadoPago = "En verificación";
                await _db.SaveChangesAsync();
            }

            _carrito.Limpiar(HttpContext.Session);

            return RedirectToAction(AccionConfirmacion, new { idVenta });
        }

        // POST /Carrito/ConfirmarDirecto — para Efectivo/Tarjeta
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarDirecto()
        {
            var (_, cliente) = await ObtenerClienteAsync();
            if (cliente == null) return Unauthorized();

            var vm = ObtenerVmDesdeTempData();
            var items = _carrito.ObtenerCarrito(HttpContext.Session);
            vm.Items = items;

            int idVenta = await _venta.RegistrarVentaAsync(cliente.IdCliente, vm, items);
            _carrito.Limpiar(HttpContext.Session);

            return RedirectToAction(AccionConfirmacion, new { idVenta });
        }

        // GET /Carrito/Confirmacion
        public async Task<IActionResult> Confirmacion(int idVenta)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(AccionMisPedidos);

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
            var (_, cliente) = await ObtenerClienteAsync();
            if (cliente == null) return RedirectToAction(AccionIndex, "Catalogo");

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
            if (!ModelState.IsValid)
                return RedirectToAction(AccionMisPedidos);

            var (_, cliente) = await ObtenerClienteAsync();
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