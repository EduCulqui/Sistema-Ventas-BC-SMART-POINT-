using Microsoft.AspNetCore.Mvc;
using Sistema_BC_SMART_POINT.Data;
using Microsoft.EntityFrameworkCore;

namespace Sistema_BC_SMART_POINT.Controllers
{
    public class CatalogoController : Controller
    {
        // Cualquier visitante puede ver el catálogo
        private readonly AppDbContext _db;
        public CatalogoController(AppDbContext db) => _db = db;

        // GET catalogo
        public async Task<IActionResult> Index(int? categoriaId, string? busqueda, string? orden)
        {
            var query = _db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Estado && p.StockActual > 0)
                .AsQueryable();

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId);

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p => p.Nombre.Contains(busqueda)
                                        || p.Modelo!.Contains(busqueda));

            query = orden switch
            {
                "precio_asc" => query.OrderBy(p => p.Precio),
                "precio_desc" => query.OrderByDescending(p => p.Precio),
                _ => query.OrderBy(p => p.Nombre)
            };

            ViewBag.Categorias = await _db.Categorias.Where(c => c.Estado).ToListAsync();
            ViewBag.CategoriaId = categoriaId;
            ViewBag.Busqueda = busqueda;
            ViewBag.Orden = orden;

            return View(await query.ToListAsync());
        }

        // GET catalogo detalle
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.IdProducto == id && p.Estado);

            if (producto == null) return NotFound();
            return View(producto);
        }
    }
}
