using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Sistema_BC_SMART_POINT.Services
{
    public class VentaService
    {
        private readonly AppDbContext _db;

        public VentaService(AppDbContext db) => _db = db;

        // Valida un cupón y retorna el porcentaje de descuento (0 si no aplica)
        public async Task<(bool valido, decimal porcentaje, int? cuponId)>
            ValidarCuponAsync(string codigo)
        {
            var hoy = DateTime.Today;
            var cupon = await _db.CuponDescuento
                .FirstOrDefaultAsync(c => c.CodigoCupon == codigo
                                       && c.Estado
                                       && c.FechaInicio <= hoy
                                       && c.FechaVencimiento >= hoy);

            if (cupon == null) return (false, 0, null);
            return (true, cupon.PorcentajeDescuento, cupon.IdCuponDescuento);
        }

        // Registra la venta completa 
        public async Task<int> RegistrarVentaAsync(
            int clienteId,
            CheckoutViewModel checkout,
            List<CarritoItemViewModel> items)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                decimal subtotal = items.Sum(i => i.SubTotal);
                decimal descuento = subtotal * (checkout.DescuentoAplicado / 100m);
                decimal totalConDescuento = subtotal - descuento;
                decimal baseImponible = Math.Round(totalConDescuento / 1.18m, 2);
                decimal igv = Math.Round(totalConDescuento - baseImponible, 2);
                decimal total = totalConDescuento;

                var venta = new Venta
                {
                    ClienteId = clienteId,
                    MetodoPago = checkout.MetodoPago,
                    EstadoPago = "Pendiente",
                    SubtotalSinDescuento = subtotal,
                    IGV = Math.Round(igv, 2),
                    TotalVenta = Math.Round(total, 2),
                    CuponDescuentoId = checkout.DescuentoAplicado > 0
                                            ? (await ValidarCuponAsync(checkout.CodigoCupon!)).cuponId
                                            : null
                };
                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();

                // Insertar detalles y descontar stock
                foreach (var item in items)
                {
                    _db.DetallesVentas.Add(new DetalleVenta
                    {
                        VentaId = venta.IdVenta,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        SubTotal = item.SubTotal
                    });

                    var producto = await _db.Productos.FindAsync(item.ProductoId);
                    if (producto != null)
                        producto.StockActual -= item.Cantidad;
                }

                // Crear el envío en estado "Preparando"
                _db.Envios.Add(new Envio
                {
                    VentaId = venta.IdVenta,
                    DireccionEnvio = checkout.DireccionEnvio,
                    Ciudad = checkout.Ciudad,
                    CodigoPostal = checkout.CodigoPostal
                });

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return venta.IdVenta;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
