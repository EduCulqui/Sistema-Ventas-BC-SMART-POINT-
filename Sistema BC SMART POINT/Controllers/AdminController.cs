using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;

namespace Sistema_BC_SMART_POINT.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalProductos = await _db.Productos.CountAsync(p => p.Estado);
            ViewBag.TotalCategorias = await _db.Categorias.CountAsync(c => c.Estado);
            ViewBag.TotalProveedores = await _db.Proveedores.CountAsync(p => p.Estado);
            ViewBag.StockBajo = await _db.Productos
                .CountAsync(p => p.Estado && p.StockActual <= p.StockMinimo);
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
            TempData["Exito"] = "Categoría creada correctamente.";
            return RedirectToAction("Categorias");
        }

        public async Task<IActionResult> EditarCategoria(int id)
        {
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
            TempData["Exito"] = "Categoría actualizada.";
            return RedirectToAction("Categorias");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Estado = false;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Categoría desactivada.";
            return RedirectToAction("Categorias");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoriaDefinitivo(int id)
        {
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction("Categorias");
            }
            _db.Categorias.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Categoría eliminada permanentemente.";
            return RedirectToAction("Categorias");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCategoria(int id)
        {
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Estado = true;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Categoría activada.";
            return RedirectToAction("Categorias");
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
            TempData["Exito"] = "Proveedor creado correctamente.";
            return RedirectToAction("Proveedores");
        }

        public async Task<IActionResult> EditarProveedor(int id)
        {
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
            TempData["Exito"] = "Proveedor actualizado.";
            return RedirectToAction("Proveedores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null) return NotFound();
            prov.Estado = false;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Proveedor desactivado.";
            return RedirectToAction("Proveedores");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProveedorDefinitivo(int id)
        {
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction("Proveedores");
            }
            _db.Proveedores.Remove(prov);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Proveedor eliminado permanentemente.";
            return RedirectToAction("Proveedores");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarProveedor(int id)
        {
            var prov = await _db.Proveedores.FindAsync(id);
            if (prov == null) return NotFound();
            prov.Estado = true;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Proveedor activado.";
            return RedirectToAction("Proveedores");
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
            TempData["Exito"] = "Producto creado correctamente.";
            return RedirectToAction("Productos");
        }

        public async Task<IActionResult> EditarProducto(int id)
        {
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
            TempData["Exito"] = "Producto actualizado.";
            return RedirectToAction("Productos");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            prod.Estado = false;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Producto desactivado.";
            return RedirectToAction("Productos");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoDefinitivo(int id)
        {
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Productos");
            }
            _db.Productos.Remove(prod);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Producto eliminado permanentemente.";
            return RedirectToAction("Productos");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarProducto(int id)
        {
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            prod.Estado = true;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Producto activado.";
            return RedirectToAction("Productos");
        }

        //  VENTAS
        public async Task<IActionResult> Ventas(string? estado, string? metodo)
        {
            var query = _db.Ventas
                .Include(v => v.Cliente).ThenInclude(c => c.Usuario)
                .Include(v => v.DetallesVenta)
                .Include(v => v.Envio)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.EstadoPago == estado);

            if (!string.IsNullOrEmpty(metodo))
                query = query.Where(v => v.MetodoPago == metodo);

            var lista = await query
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            // Contador para filtros
            ViewBag.TotalPendientes = await _db.Ventas.CountAsync(v => v.EstadoPago == "Pendiente");
            ViewBag.TotalVerificando = await _db.Ventas.CountAsync(v => v.EstadoPago == "En verificación");
            ViewBag.TotalPagados = await _db.Ventas.CountAsync(v => v.EstadoPago == "Pagado");
            ViewBag.TotalRechazados = await _db.Ventas.CountAsync(v => v.EstadoPago == "Rechazado");
            ViewBag.EstadoFiltro = estado;
            ViewBag.MetodoFiltro = metodo;

            return View(lista);
        }

        public async Task<IActionResult> DetalleVenta(int id)
        {
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
            var venta = await _db.Ventas.FindAsync(ventaId);
            if (venta == null) return NotFound();

            venta.EstadoPago = accion == "aprobar" ? "Pagado" : "Rechazado";
            await _db.SaveChangesAsync();

            TempData["Exito"] = accion == "aprobar"
                ? $"Pago del pedido #{ventaId} aprobado correctamente."
                : $"Pago del pedido #{ventaId} fue rechazado.";

            return RedirectToAction("DetalleVenta", new { id = ventaId });
        }
    }
}