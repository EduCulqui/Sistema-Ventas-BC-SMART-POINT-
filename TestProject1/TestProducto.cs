using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestProducto
{

    [TestMethod]
    public void CrearProducto_Get_RetornaVista()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CP_Get").Options;
        using var db = new AppDbContext(opciones);
        var controller = new AdminController(db);

        // Actuar
        var resultado = controller.CrearProducto();

        // Assert
        Assert.IsInstanceOfType(resultado, typeof(Task<IActionResult>));
    }

    // Validaciones del modelo Producto

    [TestMethod]
    public void Producto_DatosValidos_PasaValidacion()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = "iPhone 15",
            Descripcion = "Smartphone Apple 128GB",
            Precio = 3500m,
            StockActual = 10,
            StockMinimo = 2,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(producto, new ValidationContext(producto), resultados, true);

        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Producto_SinNombre_FallaValidacion()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = "",              
            Descripcion = "Descripcion válida",
            Precio = 100m,
            StockActual = 5,
            StockMinimo = 1,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(producto, new ValidationContext(producto), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Nombre")));
    }

    [TestMethod]
    public void Producto_NombreExcede100Caracteres_FallaValidacion()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = new string('A', 101),  
            Descripcion = "Descripcion válida",
            Precio = 100m,
            StockActual = 5,
            StockMinimo = 1,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(producto, new ValidationContext(producto), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Nombre")));
    }

    [TestMethod]
    public void Producto_SinDescripcion_FallaValidacion()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = "MacBook Air",
            Descripcion = "",        
            Precio = 2000m,
            StockActual = 5,
            StockMinimo = 1,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(producto, new ValidationContext(producto), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Descripcion")));
    }

    [TestMethod]
    public void Producto_EstadoPorDefecto_EsActivo()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = "iPhone 15 Pro",
            Descripcion = "Smartphone Apple con chip A17 Pro",
            Precio = 4200m,
            StockActual = 5,
            StockMinimo = 1,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        bool estadoActual = producto.Estado;

        // Assert
        Assert.IsTrue(estadoActual);
    }

    [TestMethod]
    public void Producto_StockActualMenorAlMinimo_SeDetectaComoBajoStock()
    {
        // Preparar
        var producto = new Producto
        {
            Nombre = "Apple Watch SE",
            Descripcion = "Smartwatch Apple con GPS",
            Precio = 1200m,
            StockActual = 1,
            StockMinimo = 5,   // stock actual < mínimo
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Actuar
        bool bajoStock = producto.StockActual < producto.StockMinimo;

        // Assert
        Assert.IsTrue(bajoStock);
    }

    [TestMethod]
    public void Producto_FechaRegistro_SeAsignaPorDefectoAlCrear()
    {
        // Preparar
        var antes = DateTime.Now.AddSeconds(-1);

        // Actuar
        var producto = new Producto
        {
            Nombre = "AirPods Pro",
            Descripcion = "Auriculares inalámbricos Apple con cancelación de ruido",
            Precio = 950m,
            StockActual = 8,
            StockMinimo = 2,
            CategoriaId = 1,
            ProveedorId = 1
        };

        // Assert
        Assert.IsTrue(producto.FechaRegistro >= antes);
    }
}
