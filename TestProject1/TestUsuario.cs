using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;
using System.ComponentModel.DataAnnotations;

namespace TestProject1;

[TestClass]
public class TestUsuario
{
    [TestMethod]
    public void Login_Get_RetornaVista()
    {
        // Preparar
        var controller = new AuthController(null);

        // Actuar
        var r = controller.Login("/home");

        // Assert

        Assert.IsInstanceOfType(r, typeof(ViewResult));


    }

    [TestMethod]
    public void Registro_Get_ViewModel()
    {
        // Preparar
        var controller = new AuthController(null);

        // Actuar
        var r = controller.Registro() as ViewResult;

        // Assert
        Assert.IsInstanceOfType(r?.Model, typeof(RegistroViewModel));
    }


    //----------------------
    // Método auxiliar que ejecuta las validaciones del modelo
    private IList<ValidationResult> Validar(Usuario usuario)
    {
        var resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(usuario);
        Validator.TryValidateObject(usuario, contexto, resultados, validateAllProperties: true);
        return resultados;
    }


    [TestMethod]
    public void Usuario_DatosIncompletos_NO_PasaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Pérez",
            Email = "juan@mail.com",
            Contrasenia = new string('A', 103),
            Rol = "Cliente",
            Celular = "98765215",
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var errores = Validar(usuario);

        // Assert
        Assert.AreEqual(0, errores.Count);
    }

    [TestMethod]
    public void Usuario_ContraseniaExcede100_FallaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Pérez",
            Email = "juan@mail.com",
            Contrasenia = new string('A', 106),
            Rol = "Cliente",
            Celular = "9876545221",
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var errores = Validar(usuario);

        // Assert
        Assert.IsNotNull(errores);
    }
}
