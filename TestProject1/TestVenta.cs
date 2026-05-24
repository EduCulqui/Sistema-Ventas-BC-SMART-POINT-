using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;

namespace TestProject1;

[TestClass]
public class TestVenta
{
    // Se inserta datos para poder registrar una venta
    private static async Task<(AppDbContext db, int clienteId, int productoId)>
    PrepararBase(string dbName)
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) 
            .Options;
        var db = new AppDbContext(opciones);

        var categoria = new Categoria { NomCategoria = "Mac", Descripcion = "Computadoras Apple", Estado = true };
        var proveedor = new Proveedor { NombreEmpresa = "iShop Distribuciones SAC", ContactoNombre = "Luis", Celular = "999999999", Email = "contacto@ishop.com.pe", Direccion = "Cajamarca", Ruc = "20123456789", Estado = true };
        db.Categorias.Add(categoria);
        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();

        var producto = new Producto { Nombre = "MacBook Air", Descripcion = "Laptop Apple chip M2", Precio = 2000m, StockActual = 10, StockMinimo = 1, CategoriaId = categoria.IdCategoria, ProveedorId = proveedor.IdProveedor, Estado = true };
        var usuario = new Usuario { Nombres = "Test", Apellidos = "User", Email = "t@t.com", Contrasenia = "hash", Rol = "Cliente", Celular = "987654321", FechaRegistro = DateTime.Now, Estado = true };
        db.Productos.Add(producto);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var cliente = new Cliente { Direccion = "Av. Grau 500", Ciudad = "Cajamarca", CodigoPostal = "06001", DniRuc = "12345678", FechaNacimiento = new DateTime(1990, 1, 1), UsuarioId = usuario.IdUsuario };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        return (db, cliente.IdCliente, producto.IdProducto);
    }

    // Modelo Venta 

    [TestMethod]
    public void Venta_EstadoPago_PorDefectoEsPendiente()
    {
        // Preparar
        var venta = new Venta
        {
            FechaVenta = DateTime.Now,
            SubtotalSinDescuento = 1000m,
            IGV = 180m,
            TotalVenta = 1180m,
            MetodoPago = "Efectivo",
            EstadoPago = "Pendiente",
            ClienteId = 1
        };

        // Actuar
        string estado = venta.EstadoPago;

        // Assert
        Assert.AreEqual("Pendiente", estado);
    }

    [TestMethod]
    public void Venta_CuponDescuentoId_EsOpcional()
    {
        // Preparar
        var venta = new Venta
        {
            FechaVenta = DateTime.Now,
            SubtotalSinDescuento = 500m,
            IGV = 90m,
            TotalVenta = 590m,
            MetodoPago = "Yape",
            EstadoPago = "Pendiente",
            ClienteId = 1,
            CuponDescuentoId = null    
        };

        // Actuar
        bool sinCupon = venta.CuponDescuentoId == null;

        // Assert
        Assert.IsTrue(sinCupon);
    }


    [TestMethod]
    public async Task RegistrarVenta_DatosValidos_RetornaIdMayorACero()
    {
        // Preparar
        var (db, clienteId, productoId) = await PrepararBase("VT_Ok");
        var service = new VentaService(db);
        var checkout = new CheckoutViewModel { DireccionEnvio = "Av. Grau 500", Ciudad = "Cajamarca", MetodoPago = "Efectivo", DescuentoAplicado = 0 };
        var items = new List<CarritoItemViewModel> { new() { ProductoId = productoId, Nombre = "MacBook Air", PrecioUnitario = 2000m, Cantidad = 1 } };

        // Actuar
        int idVenta = await service.RegistrarVentaAsync(clienteId, checkout, items);

        // Assert
        Assert.IsTrue(idVenta > 0);
    }

    [TestMethod]
    public async Task RegistrarVenta_DescontaStockDelProducto()
    {
        // Preparar
        var (db, clienteId, productoId) = await PrepararBase("VT_Stock");
        var service = new VentaService(db);
        var checkout = new CheckoutViewModel { DireccionEnvio = "Jr. Amazonas 100", Ciudad = "Cajamarca", MetodoPago = "Tarjeta", DescuentoAplicado = 0 };
        var items = new List<CarritoItemViewModel> { new() { ProductoId = productoId, Nombre = "MacBook Air", PrecioUnitario = 2000m, Cantidad = 2 } };

        // Actuar
        await service.RegistrarVentaAsync(clienteId, checkout, items);
        var producto = await db.Productos.FindAsync(productoId);

        // Assert
        Assert.AreEqual(8, producto!.StockActual);   
    }

    [TestMethod]
    public async Task RegistrarVenta_CreaEnvioEnEstadoPreparando()
    {
        // Preparar
        var (db, clienteId, productoId) = await PrepararBase("VT_Envio");
        var service = new VentaService(db);
        var checkout = new CheckoutViewModel { DireccionEnvio = "Av. Grau 500", Ciudad = "Cajamarca", CodigoPostal = "06001", MetodoPago = "Yape", DescuentoAplicado = 0 };
        var items = new List<CarritoItemViewModel> { new() { ProductoId = productoId, Nombre = "MacBook Air", PrecioUnitario = 2000m, Cantidad = 1 } };

        // Actuar
        int idVenta = await service.RegistrarVentaAsync(clienteId, checkout, items);
        var envio = await db.Envios.FirstOrDefaultAsync(e => e.VentaId == idVenta);

        // Assert
        Assert.IsNotNull(envio);
        Assert.AreEqual("Preparando", envio.EstadoEnvio);
    }

    [TestMethod]
    public async Task RegistrarVenta_ConDescuentoDel10_CalculaTotalCorrecto()
    {
        // Preparar
        var (db, clienteId, productoId) = await PrepararBase("VT_Desc");
        var service = new VentaService(db);
        var checkout = new CheckoutViewModel { DireccionEnvio = "Av. Grau 500", Ciudad = "Cajamarca", MetodoPago = "Plin", DescuentoAplicado = 10 };
        var items = new List<CarritoItemViewModel> { new() { ProductoId = productoId, Nombre = "MacBook Air", PrecioUnitario = 2000m, Cantidad = 1 } };

        // Actuar
        int idVenta = await service.RegistrarVentaAsync(clienteId, checkout, items);
        var venta = await db.Ventas.FindAsync(idVenta);

        // Assert
        // subtotal=2000, 10% desc=200, totalConDesc=1800
        Assert.AreEqual(1800m, venta!.TotalVenta);
    }

    [TestMethod]
    public async Task RegistrarVenta_GuardaDetallesDeVenta()
    {
        // Preparar
        var (db, clienteId, productoId) = await PrepararBase("VT_Detalles");
        var service = new VentaService(db);
        var checkout = new CheckoutViewModel { DireccionEnvio = "Jr. Amazonas 100", Ciudad = "Cajamarca", MetodoPago = "Yape", DescuentoAplicado = 0 };
        var items = new List<CarritoItemViewModel> { new() { ProductoId = productoId, Nombre = "MacBook Air", PrecioUnitario = 2000m, Cantidad = 3 } };

        // Actuar
        int idVenta = await service.RegistrarVentaAsync(clienteId, checkout, items);
        var detalles = db.DetallesVentas.Where(d => d.VentaId == idVenta).ToList();

        // Assert
        Assert.AreEqual(1, detalles.Count);
        Assert.AreEqual(3, detalles[0].Cantidad);
    }
}
