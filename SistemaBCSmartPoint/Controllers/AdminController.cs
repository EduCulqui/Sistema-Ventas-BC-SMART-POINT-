using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using System.Security.Claims;

namespace Sistema_BC_SMART_POINT.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        private const string ExitoKey = "Exito";
        private const string ErrorKey = "Error";
        private const string AccionCategorias = "Categorias";
        private const string AccionProveedores = "Proveedores";
        private const string AccionProductos = "Productos";
        private const string AccionEnvios = "Envios";
        private const string AccionClientes = "Clientes";
        private const string AccionCupones = "Cupones";

        // Constantes de estados de pago
        private const string EstadoPagado = "Pagado";
        private const string EstadoPendiente = "Pendiente";
        private const string EstadoVerificando = "En verificación";
        private const string EstadoRechazado = "Rechazado";

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Dashboard");

            ViewBag.TotalProductos = await _db.Productos.CountAsync(p => p.Estado);
            ViewBag.TotalCategorias = await _db.Categorias.CountAsync(c => c.Estado);
            ViewBag.TotalProveedores = await _db.Proveedores.CountAsync(p => p.Estado);
            ViewBag.StockBajo = await _db.Productos
                .CountAsync(p => p.Estado && p.StockActual <= p.StockMinimo);

            // Ventas del mes actual
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Local);

            ViewBag.VentasHoy = await _db.Ventas.CountAsync(v => v.FechaVenta.Date == hoy);
            ViewBag.VentasMes = await _db.Ventas.CountAsync(v => v.FechaVenta >= inicioMes);
            ViewBag.IngresosMes = await _db.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.EstadoPago == EstadoPagado)
                .SumAsync(v => (decimal?)v.TotalVenta) ?? 0;
            ViewBag.VentasPendientes = await _db.Ventas
                .CountAsync(v => v.EstadoPago == EstadoPendiente || v.EstadoPago == EstadoVerificando);

            // Ventas últimos 30 días para gráfico
            var inicio30 = hoy.AddDays(-29);
            var ventas30 = await _db.Ventas
                .Where(v => v.FechaVenta.Date >= inicio30)
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.TotalVenta), Cantidad = g.Count() })
                .OrderBy(g => g.Fecha)
                .ToListAsync();

            // Generar labels y datos para todos los días (incluso sin ventas)
            var labels = new List<string>();
            var montos = new List<decimal>();
            var cantidades = new List<int>();
            for (int i = 0; i < 30; i++)
            {
                var fecha = inicio30.AddDays(i);
                var venta = ventas30.FirstOrDefault(v => v.Fecha == fecha);
                labels.Add(fecha.ToString("dd/MM"));
                montos.Add(venta?.Total ?? 0);
                cantidades.Add(venta?.Cantidad ?? 0);
            }

            ViewBag.GraficoLabels = System.Text.Json.JsonSerializer.Serialize(labels);
            ViewBag.GraficoMontos = System.Text.Json.JsonSerializer.Serialize(montos);
            ViewBag.GraficoCantidades = System.Text.Json.JsonSerializer.Serialize(cantidades);

            // Ventas por estado para dona
            ViewBag.EstadoPendiente = await _db.Ventas.CountAsync(v => v.EstadoPago == EstadoPendiente);
            ViewBag.EstadoVerificando = await _db.Ventas.CountAsync(v => v.EstadoPago == EstadoVerificando);
            ViewBag.EstadoPagado = await _db.Ventas.CountAsync(v => v.EstadoPago == EstadoPagado);
            ViewBag.EstadoRechazado = await _db.Ventas.CountAsync(v => v.EstadoPago == EstadoRechazado);

            return View();
        }

        //  CATEGORÍAS

        public async Task<IActionResult> Categorias()
        {
            var lista = await _db.Categorias
                .OrderBy(c => c.NomCategoria)
                .ToListAsync();
            return View(lista);
        }

        public IActionResult CrearCategoria() => View(new Categoria());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria(Categoria model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Categorias.Add(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Categoría creada correctamente.";
            return RedirectToAction(AccionCategorias);
        }

        public async Task<IActionResult> EditarCategoria(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCategorias);
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCategoria(Categoria model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Categorias.Update(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Categoría actualizada.";
            return RedirectToAction(AccionCategorias);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCategorias);
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Estado = false;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Categoría desactivada.";
            return RedirectToAction(AccionCategorias);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoriaDefinitivo(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCategorias);
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null)
            {
                TempData[ErrorKey] = "Categoría no encontrada.";
                return RedirectToAction(AccionCategorias);
            }
            _db.Categorias.Remove(cat);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Categoría eliminada permanentemente.";
            return RedirectToAction(AccionCategorias);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCategoria(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCategorias);
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Estado = true;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Categoría activada.";
            return RedirectToAction(AccionCategorias);
        }


        //  PROVEEDORES
        public async Task<IActionResult> Proveedores()
        {
            var lista = await _db.Proveedores
                .OrderBy(p => p.NombreEmpresa)
                .ToListAsync();
            return View(lista);
        }

        public IActionResult CrearProveedor() => View(new Proveedor());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProveedor(Proveedor model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Proveedores.Add(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Proveedor creado correctamente.";
            return RedirectToAction(AccionProveedores);
        }

        public async Task<IActionResult> EditarProveedor(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProveedores);
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null) return NotFound();
            return View(prov);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProveedor(Proveedor model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Proveedores.Update(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Proveedor actualizado.";
            return RedirectToAction(AccionProveedores);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProveedores);
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null) return NotFound();
            prov.Estado = false;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Proveedor desactivado.";
            return RedirectToAction(AccionProveedores);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProveedorDefinitivo(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProveedores);
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null)
            {
                TempData[ErrorKey] = "Proveedor no encontrado.";
                return RedirectToAction(AccionProveedores);
            }
            _db.Proveedores.Remove(prov);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Proveedor eliminado permanentemente.";
            return RedirectToAction(AccionProveedores);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarProveedor(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProveedores);
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null) return NotFound();
            prov.Estado = true;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Proveedor activado.";
            return RedirectToAction(AccionProveedores);
        }


        //  PRODUCTOS
        public async Task<IActionResult> Productos()
        {
            var lista = await _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> CrearProducto()
        {
            ViewBag.Categorias = await _db.Categorias.Where(c => c.Estado).ToListAsync();
            ViewBag.Proveedores = await _db.Proveedores.Where(p => p.Estado).ToListAsync();
            return View(new Producto { FechaRegistro = DateTime.Now, Estado = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(Producto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.Where(c => c.Estado).ToListAsync();
                ViewBag.Proveedores = await _db.Proveedores.Where(p => p.Estado).ToListAsync();
                return View(model);
            }
            model.FechaRegistro = DateTime.Now;
            _db.Productos.Add(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Producto creado correctamente.";
            return RedirectToAction(AccionProductos);
        }

        public async Task<IActionResult> EditarProducto(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProductos);
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            ViewBag.Categorias = await _db.Categorias.Where(c => c.Estado).ToListAsync();
            ViewBag.Proveedores = await _db.Proveedores.Where(p => p.Estado).ToListAsync();
            return View(prod);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(Producto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.Where(c => c.Estado).ToListAsync();
                ViewBag.Proveedores = await _db.Proveedores.Where(p => p.Estado).ToListAsync();
                return View(model);
            }
            _db.Productos.Update(model);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Producto actualizado.";
            return RedirectToAction(AccionProductos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProductos);
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            prod.Estado = false;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Producto desactivado.";
            return RedirectToAction(AccionProductos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoDefinitivo(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProductos);
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null)
            {
                TempData[ErrorKey] = "Producto no encontrado.";
                return RedirectToAction(AccionProductos);
            }
            _db.Productos.Remove(prod);
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Producto eliminado permanentemente.";
            return RedirectToAction(AccionProductos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarProducto(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionProductos);
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            prod.Estado = true;
            await _db.SaveChangesAsync();
            TempData[ExitoKey] = "Producto activado.";
            return RedirectToAction(AccionProductos);
        }

        //  VENTAS
        public async Task<IActionResult> Ventas(string? estado, string? metodo, int pagina = 1, int tamano = 10)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Dashboard");
            var query = _db.Ventas
                .Include(v => v.Cliente).ThenInclude(c => c.Usuario)
                .Include(v => v.DetallesVenta)
                .Include(v => v.Envio)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.EstadoPago == estado);
            if (!string.IsNullOrEmpty(metodo))
                query = query.Where(v => v.MetodoPago == metodo);

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(v => v.FechaVenta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            ViewBag.TotalPendientes = await _db.Ventas.CountAsync(v => v.EstadoPago == "Pendiente");
            ViewBag.TotalVerificando = await _db.Ventas.CountAsync(v => v.EstadoPago == "En verificación");
            ViewBag.TotalPagados = await _db.Ventas.CountAsync(v => v.EstadoPago == "Pagado");
            ViewBag.TotalRechazados = await _db.Ventas.CountAsync(v => v.EstadoPago == "Rechazado");
            ViewBag.EstadoFiltro = estado;
            ViewBag.MetodoFiltro = metodo;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamano);
            ViewBag.TamanoPagina = tamano;
            ViewBag.TotalRegistros = total;

            return View(lista);
        }

        public async Task<IActionResult> DetalleVenta(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction("Ventas");
            var venta = await _db.Ventas
                .Include(v => v.Cliente).ThenInclude(c => c.Usuario)
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .Include(v => v.CuponDescuento)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null) return NotFound();
            return View(venta);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidarPago(int ventaId, string accion)
        {
            if (!ModelState.IsValid) return RedirectToAction("DetalleVenta", new { id = ventaId });
            var venta = await _db.Ventas.FindAsync(ventaId);
            if (venta == null) return NotFound();

            venta.EstadoPago = accion == "aprobar" ? "Pagado" : "Rechazado";
            await _db.SaveChangesAsync();

            TempData[ExitoKey] = accion == "aprobar"
                ? $"Pago del pedido #{ventaId} aprobado correctamente."
                : $"Pago del pedido #{ventaId} fue rechazado.";

            return RedirectToAction("DetalleVenta", new { id = ventaId });
        }


        //  ENVÍOS
        public async Task<IActionResult> Envios(string? estado)
        {
            var query = _db.Envios
                .Include(e => e.Venta).ThenInclude(v => v.Cliente).ThenInclude(c => c.Usuario)
                .Include(e => e.Administrador).ThenInclude(a => a.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(e => e.EstadoEnvio == estado);

            var lista = await query
                .OrderByDescending(e => e.IdEnvio)
                .ToListAsync();

            ViewBag.TotalPreparando = await _db.Envios.CountAsync(e => e.EstadoEnvio == "Preparando");
            ViewBag.TotalDespachado = await _db.Envios.CountAsync(e => e.EstadoEnvio == "Despachado");
            ViewBag.TotalEnCamino = await _db.Envios.CountAsync(e => e.EstadoEnvio == "En camino");
            ViewBag.TotalEntregado = await _db.Envios.CountAsync(e => e.EstadoEnvio == "Entregado");
            ViewBag.EstadoFiltro = estado;

            return View(lista);
        }

        public async Task<IActionResult> EditarEnvio(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionEnvios);
            var envio = await _db.Envios
                .Include(e => e.Venta).ThenInclude(v => v.Cliente).ThenInclude(c => c.Usuario)
                .Include(e => e.Venta).ThenInclude(v => v.DetallesVenta)
                .FirstOrDefaultAsync(e => e.IdEnvio == id);

            if (envio == null) return NotFound();

            var usuarioId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
            var admin = await _db.Administradores.FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);
            ViewBag.AdminId = admin?.IdAdministrador;

            return View(envio);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEnvio(int idEnvio, string estadoEnvio,
            string? empresaTransporte, string? numeroSeguimiento,
            DateTime? fechaEnvio, DateTime? fechaEntregaEstimada, int? adminId)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionEnvios);
            var envio = await _db.Envios.FindAsync(idEnvio);
            if (envio == null) return NotFound();

            envio.EstadoEnvio = estadoEnvio;
            envio.EmpresaTransporte = empresaTransporte;
            envio.NumeroSeguimiento = numeroSeguimiento;
            envio.FechaEnvio = fechaEnvio;
            envio.FechaEntregaEstimada = fechaEntregaEstimada;
            envio.AdministradorId = adminId;

            if (estadoEnvio == "Despachado" && envio.FechaEnvio == null)
                envio.FechaEnvio = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData[ExitoKey] = $"Envío #{idEnvio} actualizado correctamente.";
            return RedirectToAction(AccionEnvios);
        }


        //  CLIENTES
        public async Task<IActionResult> Clientes(string? busqueda)
        {
            var query = _db.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Ventas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(c =>
                    c.Usuario.Nombres.Contains(busqueda) ||
                    c.Usuario.Apellidos.Contains(busqueda) ||
                    c.Usuario.Email.Contains(busqueda) ||
                    c.DniRuc.Contains(busqueda));

            var lista = await query
                .OrderByDescending(c => c.IdCliente)
                .ToListAsync();

            ViewBag.Busqueda = busqueda;
            ViewBag.TotalActivos = await _db.Usuarios.CountAsync(u => u.Rol == "Cliente" && u.Estado);
            ViewBag.TotalInactivos = await _db.Usuarios.CountAsync(u => u.Rol == "Cliente" && !u.Estado);

            return View(lista);
        }

        public async Task<IActionResult> DetalleCliente(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionClientes);
            var cliente = await _db.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null) return NotFound();

            var ventas = await _db.Ventas
                .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
                .Include(v => v.Envio)
                .Where(v => v.ClienteId == cliente.IdCliente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            cliente.Ventas = ventas;

            return View(cliente);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstadoCliente(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionClientes);
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Estado = !usuario.Estado;
            await _db.SaveChangesAsync();

            TempData[ExitoKey] = usuario.Estado
                ? "Cliente activado correctamente."
                : "Cliente desactivado correctamente.";

            return RedirectToAction(AccionClientes);
        }


        //  CUPON DESCUENTO
        public async Task<IActionResult> Cupones()
        {
            var lista = await _db.CuponDescuento
                .OrderByDescending(c => c.FechaCreación)
                .ToListAsync();

            ViewBag.TotalActivos = lista.Count(c => c.Estado && c.FechaVencimiento >= DateTime.Today);
            ViewBag.TotalVencidos = lista.Count(c => c.FechaVencimiento < DateTime.Today);

            return View(lista);
        }

        public IActionResult CrearCupon()
        {
            return View(new CuponDescuento
            {
                Estado = true,
                FechaInicio = DateTime.Today,
                FechaVencimiento = DateTime.Today.AddMonths(1),
                FechaCreación = DateTime.Now
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCupon(CuponDescuento model)
        {
            if (await _db.CuponDescuento.AnyAsync(c => c.CodigoCupon == model.CodigoCupon))
            {
                ModelState.AddModelError("CodigoCupon", "Este código ya existe.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            model.CodigoCupon = model.CodigoCupon.ToUpper().Trim();
            model.FechaCreación = DateTime.Now;
            _db.CuponDescuento.Add(model);
            await _db.SaveChangesAsync();

            TempData[ExitoKey] = $"Cupón {model.CodigoCupon} creado correctamente.";
            return RedirectToAction(AccionCupones);
        }

        public async Task<IActionResult> EditarCupon(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCupones);
            var cupon = await _db.CuponDescuento.FindAsync(id);
            if (cupon == null) return NotFound();
            return View(cupon);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCupon(CuponDescuento model)
        {
            if (await _db.CuponDescuento.AnyAsync(c =>
                    c.CodigoCupon == model.CodigoCupon &&
                    c.IdCuponDescuento != model.IdCuponDescuento))
            {
                ModelState.AddModelError("CodigoCupon", "Este código ya está en uso.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            model.CodigoCupon = model.CodigoCupon.ToUpper().Trim();
            _db.CuponDescuento.Update(model);
            await _db.SaveChangesAsync();

            TempData[ExitoKey] = $"Cupón {model.CodigoCupon} actualizado.";
            return RedirectToAction(AccionCupones);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstadoCupon(int id)
        {
            if (!ModelState.IsValid) return RedirectToAction(AccionCupones);
            var cupon = await _db.CuponDescuento.FindAsync(id);
            if (cupon == null) return NotFound();

            cupon.Estado = !cupon.Estado;
            await _db.SaveChangesAsync();

            TempData[ExitoKey] = cupon.Estado
                ? $"Cupón {cupon.CodigoCupon} activado."
                : $"Cupón {cupon.CodigoCupon} desactivado.";

            return RedirectToAction(AccionCupones);
        }

    }
}