using Sistema_BC_SMART_POINT.Models;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestEnvio
{
    // Validaciones del modelo Envio
    [TestMethod]
    public void Envio_DatosValidos_PasaValidacion()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "Av. Grau 500",
            Ciudad = "Cajamarca",
            CodigoPostal = "06001",
            VentaId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(envio, new ValidationContext(envio), resultados, true);
        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Envio_EstadoPorDefecto_EsPreparando()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "Jr. Amazonas 100",
            Ciudad = "Lima",
            VentaId = 1
        };
        // Actuar
        string estado = envio.EstadoEnvio;
        // Assert
        Assert.AreEqual("Preparando", estado);
    }

    [TestMethod]
    public void Envio_SinDireccion_FallaValidacion()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "",         
            Ciudad = "Lima",
            VentaId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(envio, new ValidationContext(envio), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("DireccionEnvio")));
    }

    [TestMethod]
    public void Envio_SinCiudad_FallaValidacion()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "Av. Lima 1",
            Ciudad = "",     
            VentaId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(envio, new ValidationContext(envio), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Ciudad")));
    }

    [TestMethod]
    public void Envio_CodigoPostalEsOpcional_PasaValidacionSinEl()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "Av. Grau 200",
            Ciudad = "Cajamarca",      
            CodigoPostal = null,          
            VentaId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(envio, new ValidationContext(envio), resultados, true);
        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Envio_CambioDeEstadoADespachado_SeActualizaCorrectamente()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = "Av. Grau 500",
            Ciudad = "Cajamarca",
            VentaId = 1
        };
        // Actuar
        envio.EstadoEnvio = "Despachado";
        envio.FechaEnvio = DateTime.Now;
        envio.EmpresaTransporte = "Shalom";       
        envio.NumeroSeguimiento = "SHA123456";
        // Assert
        Assert.AreEqual("Despachado", envio.EstadoEnvio);
        Assert.IsNotNull(envio.FechaEnvio);
        Assert.AreEqual("Shalom", envio.EmpresaTransporte);
    }

    [TestMethod]
    public void Envio_DireccionExcede200Caracteres_FallaValidacion()
    {
        // Preparar
        var envio = new Envio
        {
            DireccionEnvio = new string('A', 201),  
            Ciudad = "Cajamarca",
            VentaId = 1
        };
        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(envio, new ValidationContext(envio), resultados, true);
        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("DireccionEnvio")));
    }
}
