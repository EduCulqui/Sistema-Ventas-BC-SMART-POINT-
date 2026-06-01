using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestProveedor
{

    [TestMethod]
    public void CrearProveedor_Get_RetornaVista()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("PROV_Get").Options;
        using var db = new AppDbContext(opciones);
        var controller = new AdminController(db);

        // Actuar
        var resultado = controller.CrearProveedor();

        // Assert
        Assert.IsInstanceOfType(resultado, typeof(ViewResult));
    }

    // Validaciones del modelo Proveedor 

    [TestMethod]
    public void Proveedor_DatosValidos_PasaValidacion()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = "iShop Distribuciones SAC",
            ContactoNombre = "Luis Torres",
            Celular = "987654321",
            Email = "contacto@ishop.com.pe",
            Direccion = "Av. Javier Prado 500, Lima",
            Ruc = "20123456789",
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(proveedor, new ValidationContext(proveedor), resultados, true);

        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Proveedor_SinNombreEmpresa_FallaValidacion()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = "",     
            ContactoNombre = "Luis Torres",
            Celular = "987654321",
            Email = "contacto@ishop.com.pe",
            Direccion = "Av. Javier Prado 500, Lima",
            Ruc = "20123456789",
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(proveedor, new ValidationContext(proveedor), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("NombreEmpresa")));
    }

    [TestMethod]
    public void Proveedor_SinRuc_FallaValidacion()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = "Apple Premium Reseller SAC",
            ContactoNombre = "Pedro Ruiz",
            Celular = "912345678",
            Email = "ventas@applereseller.pe",
            Direccion = "Jr. Comercio 100, Cajamarca",
            Ruc = "",                
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(proveedor, new ValidationContext(proveedor), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Ruc")));
    }

    [TestMethod]
    public void Proveedor_RucExcede11Caracteres_FallaValidacion()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = "Mac Center Perú SAC",
            ContactoNombre = "María Quispe",
            Celular = "999888777",
            Email = "info@maccenter.pe",
            Direccion = "Av. Larco 200, Miraflores",
            Ruc = "201234567890",        
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(proveedor, new ValidationContext(proveedor), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Ruc")));
    }

    [TestMethod]
    public void Proveedor_NombreEmpresaExcede100Caracteres_FallaValidacion()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = new string('X', 101),
            ContactoNombre = "Juan Perez",
            Celular = "987654321",
            Email = "largo@empresa.com",
            Direccion = "Jr. Largo 999, Cajamarca",
            Ruc = "20999999999",
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(proveedor, new ValidationContext(proveedor), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("NombreEmpresa")));
    }

    [TestMethod]
    public void Proveedor_EstadoPorDefecto_EsActivo()
    {
        // Preparar
        var proveedor = new Proveedor
        {
            NombreEmpresa = "iCenter Cajamarca SAC",
            ContactoNombre = "Ana Diaz",
            Celular = "912345678",
            Email = "icenter@cajamarca.pe",
            Direccion = "Jr. Comercio 100, Cajamarca",
            Ruc = "20999999999"
        };

        // Actuar
        bool estadoActual = proveedor.Estado;

        // Assert
        Assert.IsTrue(estadoActual);
    }
}
