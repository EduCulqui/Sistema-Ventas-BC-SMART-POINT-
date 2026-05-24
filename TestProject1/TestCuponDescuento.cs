using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Services;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestCuponDescuento
{

    [TestMethod]
    public void CrearCupon_Get_RetornaVista()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CUP_Get").Options;
        using var db = new AppDbContext(opciones);
        var controller = new AdminController(db);

        var resultado = controller.CrearCupon();

        Assert.IsInstanceOfType(resultado, typeof(ViewResult));
    }

    // Validaciones del modelo CuponDescuento 

    [TestMethod]
    public void CuponDescuento_DatosValidos_PasaValidacion()
    {
        var cupon = new CuponDescuento
        {
            CodigoCupon = "APPLE10",
            PorcentajeDescuento = 10,
            FechaInicio = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(30),
            Estado = true,
            FechaCreación = DateTime.Now
        };

        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(cupon, new ValidationContext(cupon), resultados, true);

        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void CuponDescuento_CodigoExcede12Caracteres_FallaValidacion()
    {
        var cupon = new CuponDescuento
        {
            CodigoCupon = "CODIGOMUYLARGOOO",   
            PorcentajeDescuento = 10,
            FechaInicio = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(10),
            Estado = true,
            FechaCreación = DateTime.Now
        };

        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(cupon, new ValidationContext(cupon), resultados, true);

        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("CodigoCupon")));
    }

    [TestMethod]
    public void CuponDescuento_PorcentajeCero_FallaValidacionPorRangeMinimo()
    {
        var cupon = new CuponDescuento
        {
            CodigoCupon = "IPHONE0",
            PorcentajeDescuento = 0,   
            FechaInicio = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(10),
            Estado = true,
            FechaCreación = DateTime.Now
        };

        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(cupon, new ValidationContext(cupon), resultados, true);

        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("PorcentajeDescuento")));
    }

    [TestMethod]
    public void CuponDescuento_Porcentaje96_FallaValidacionPorRangeMaximo()
    {
        var cupon = new CuponDescuento
        {
            CodigoCupon = "MACBOOK96",
            PorcentajeDescuento = 96,  
            FechaInicio = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(10),
            Estado = true,
            FechaCreación = DateTime.Now
        };

        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(cupon, new ValidationContext(cupon), resultados, true);

        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("PorcentajeDescuento")));
    }



    [TestMethod]
    public async Task ValidarCupon_CuponVigente_RetornaValidoConPorcentaje()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CUP_Vigente").Options;
        using var db = new AppDbContext(opciones);
        db.CuponDescuento.Add(new CuponDescuento
        {
            CodigoCupon = "IPAD10",
            PorcentajeDescuento = 10,
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaVencimiento = DateTime.Today.AddDays(10),
            Estado = true,
            FechaCreación = DateTime.Now
        });
        await db.SaveChangesAsync();
        var service = new VentaService(db);

        var (valido, porcentaje, _) = await service.ValidarCuponAsync("IPAD10");

        Assert.IsTrue(valido);
        Assert.AreEqual(10, porcentaje);
    }

    [TestMethod]
    public async Task ValidarCupon_CuponVencido_RetornaInvalido()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CUP_Vencido").Options;
        using var db = new AppDbContext(opciones);
        db.CuponDescuento.Add(new CuponDescuento
        {
            CodigoCupon = "WATCH20",
            PorcentajeDescuento = 20,
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaVencimiento = DateTime.Today.AddDays(-1),  // ya venció
            Estado = true,
            FechaCreación = DateTime.Now
        });
        await db.SaveChangesAsync();
        var service = new VentaService(db);

        var (valido, porcentaje, _) = await service.ValidarCuponAsync("WATCH20");

        Assert.IsFalse(valido);
        Assert.AreEqual(0, porcentaje);
    }

    [TestMethod]
    public async Task ValidarCupon_CuponDesactivado_RetornaInvalido()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CUP_Desact").Options;
        using var db = new AppDbContext(opciones);
        db.CuponDescuento.Add(new CuponDescuento
        {
            CodigoCupon = "AIRPODS",
            PorcentajeDescuento = 15,
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaVencimiento = DateTime.Today.AddDays(10),
            Estado = false,                
            FechaCreación = DateTime.Now
        });
        await db.SaveChangesAsync();
        var service = new VentaService(db);

        var (valido, _, _) = await service.ValidarCuponAsync("AIRPODS");

        Assert.IsFalse(valido);
    }

    [TestMethod]
    public async Task ValidarCupon_CodigoInexistente_RetornaInvalido()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CUP_NoExiste").Options;
        using var db = new AppDbContext(opciones);
        var service = new VentaService(db);

        var (valido, porcentaje, cuponId) = await service.ValidarCuponAsync("NOEXISTE");

        Assert.IsFalse(valido);
        Assert.AreEqual(0, porcentaje);
        Assert.IsNull(cuponId);
    }
}
