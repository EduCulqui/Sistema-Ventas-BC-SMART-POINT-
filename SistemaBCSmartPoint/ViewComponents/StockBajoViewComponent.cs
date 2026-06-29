using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;

namespace Sistema_BC_SMART_POINT.ViewComponents
{
    public class StockBajoViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        public StockBajoViewComponent(AppDbContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var productos = await _db.Productos
                .Where(p => p.Estado && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual)
                .Select(p => new { p.Nombre, p.StockActual, p.StockMinimo })
                .ToListAsync();

            return View(productos);
        }
    }
}