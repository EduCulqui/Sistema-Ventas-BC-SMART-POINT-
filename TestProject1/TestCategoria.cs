using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestCategoria
{
    [TestMethod]
    public void CrearCategoria_Get_RetornaVista()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CAT_Get").Options;
        using var db = new AppDbContext(opciones);
        var controller = new AdminController(db);
        // Actuar
        var resultado = controller.CrearCategoria();
        // Assert
        Assert.IsInstanceOfType(resultado, typeof(ViewResult));
    }

    // Validaciones del modelo Categoria 
    [TestMethod]
    public void Categoria_DatosValidos_PasaValidacion()
    {
        // Preparar
        var categoria = new Categoria
        {
            NomCategoria = "iPhones",
            Descripcion = "Teléfonos inteligentes Apple de última generación",
            Estado = true
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(categoria, new ValidationContext(categoria), resultados, true);
        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Categoria_SinNombre_FallaValidacion()
    {
        // Preparar
        var categoria = new Categoria
        {
            NomCategoria = "",       
            Descripcion = "Descripción válida",
            Estado = true
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(categoria, new ValidationContext(categoria), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("NomCategoria")));
    }

    [TestMethod]
    public void Categoria_NombreExcede50Caracteres_FallaValidacion()
    {
        // Preparar
        var categoria = new Categoria
        {
            NomCategoria = new string('C', 51),  
            Descripcion = "Descripción válida",
            Estado = true
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(categoria, new ValidationContext(categoria), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("NomCategoria")));
    }

    [TestMethod]
    public void Categoria_DescripcionExcede500Caracteres_FallaValidacion()
    {
        // Preparar
        var categoria = new Categoria
        {
            NomCategoria = "Accesorios Apple",
            Descripcion = new string('D', 501),  
            Estado = true
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(categoria, new ValidationContext(categoria), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Descripcion")));
    }

    [TestMethod]
    public void Categoria_SinDescripcion_FallaValidacion()
    {
        // Preparar
        var categoria = new Categoria
        {
            NomCategoria = "MacBook",
            Descripcion = "", 
            Estado = true
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(categoria, new ValidationContext(categoria), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Descripcion")));
    }
}
