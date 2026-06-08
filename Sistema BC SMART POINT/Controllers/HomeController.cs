using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Models.ViewModels;

namespace Sistema_BC_SMART_POINT.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoriaId, string? busqueda, string? orden)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Estado && p.StockActual > 0)
                .AsQueryable();

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId);

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) || p.Modelo!.Contains(busqueda));

            query = orden switch
            {
                "precio_asc"  => query.OrderBy(p => p.Precio),
                "precio_desc" => query.OrderByDescending(p => p.Precio),
                _             => query.OrderBy(p => p.Nombre)
            };

            var vm = new HomeViewModel
            {
                ProductosFiltrados = await query.ToListAsync(),

                Categorias = await _context.Categorias
                    .Where(c => c.Estado)
                    .OrderBy(c => c.NomCategoria)
                    .ToListAsync(),

                CategoriaActual = categoriaId,
                BusquedaActual  = busqueda ?? "",
                OrdenActual     = orden ?? ""
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
