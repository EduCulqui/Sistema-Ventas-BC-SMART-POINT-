using Sistema_BC_SMART_POINT.Models;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestDetalleVenta
{
    // Validaciones del modelo DetalleVenta 
    [TestMethod]
    public void DetalleVenta_DatosValidos_PasaValidacion()
    {
        // Preparar
        var detalle = new DetalleVenta
        {
            Cantidad = 2,
            PrecioUnitario = 999,   
            SubTotal = 1998,
            VentaId = 1,
            ProductoId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(detalle, new ValidationContext(detalle), resultados, true);
        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void DetalleVenta_SubTotal_EsCantidadPorPrecioUnitario()
    {
        // Preparar
        int cantidad = 3;
        decimal precioUnitario = 250m;   
        // Actuar
        decimal subtotal = cantidad * precioUnitario;
        var detalle = new DetalleVenta { Cantidad = cantidad, PrecioUnitario = precioUnitario, SubTotal = subtotal, VentaId = 1, ProductoId = 1 };
        // Assert
        Assert.AreEqual(750m, detalle.SubTotal);
    }

    [TestMethod]
    public void DetalleVenta_CantidadUno_SubTotalIgualAPrecioUnitario()
    {
        // Preparar
        decimal precio = 1999.99m;  
        // Actuar
        var detalle = new DetalleVenta { Cantidad = 1, PrecioUnitario = precio, SubTotal = precio * 1, VentaId = 1, ProductoId = 1 };
        // Assert
        Assert.AreEqual(precio, detalle.SubTotal);
    }

    [TestMethod]
    public void DetalleVenta_SumaDeVariosDetalles_TotalEsAcumulado()
    {
        // Preparar
        var d1 = new DetalleVenta { Cantidad = 2, PrecioUnitario = 100m, SubTotal = 200m, VentaId = 1, ProductoId = 1 };  // Magic Mouse
        var d2 = new DetalleVenta { Cantidad = 1, PrecioUnitario = 300m, SubTotal = 300m, VentaId = 1, ProductoId = 2 };  // iPad mini
        // Actuar
        decimal totalGeneral = d1.SubTotal + d2.SubTotal;
        // Assert
        Assert.AreEqual(500m, totalGeneral);
    }

    [TestMethod]
    public void DetalleVenta_PrecioUnitarioCero_SubTotalEsCero()
    {
        // Preparar
        var detalle = new DetalleVenta { Cantidad = 5, PrecioUnitario = 0m, SubTotal = 0m * 5, VentaId = 1, ProductoId = 1 };
        // Actuar
        decimal resultado = detalle.SubTotal;
        // Assert
        Assert.AreEqual(0m, resultado);
    }
}
