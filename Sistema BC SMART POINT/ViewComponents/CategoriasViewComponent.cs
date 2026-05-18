using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;

namespace Sistema_BC_SMART_POINT.ViewComponents
{
    public class CategoriasViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        public CategoriasViewComponent(AppDbContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categorias = await _db.Categorias
                .Where(c => c.Estado)
                .OrderBy(c => c.NomCategoria)
                .ToListAsync();
            return View(categorias);
        }
    }
}