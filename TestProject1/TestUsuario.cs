using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
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
        var controller = new AuthController(null!);

        // Actuar
        var resultado = controller.Login("/home");

        // Assert
        Assert.IsInstanceOfType(resultado, typeof(ViewResult));
    }


    [TestMethod]
    public void Registro_Get_RetornaVistaConRegistroViewModel()
    {
        // Preparar
        var controller = new AuthController(null!);

        // Actuar
        var resultado = controller.Registro() as ViewResult;

        // Assert
        Assert.IsInstanceOfType(resultado?.Model, typeof(RegistroViewModel));
    }

    // Validaciones del modelo Usuario 

    [TestMethod]
    public void Usuario_DatosValidos_PasaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Pérez",
            Email = "juan@mail.com",
            Contrasenia = "claveSegura",
            Rol = "Cliente",
            Celular = "987654321",
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(usuario, new ValidationContext(usuario), resultados, true);

        // Assert
        Assert.AreEqual(0, resultados.Count);
    }

    [TestMethod]
    public void Usuario_ContraseniaExcede100Caracteres_FallaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Pérez",
            Email = "juan@mail.com",
            Contrasenia = new string('A', 101),   // máximo es 100
            Rol = "Cliente",
            Celular = "987654321",
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(usuario, new ValidationContext(usuario), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Contrasenia")));
    }

    [TestMethod]
    public void Usuario_SinEmail_FallaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Ana",
            Apellidos = "Rios",
            Email = "",           // campo requerido vacío
            Contrasenia = "clave123",
            Rol = "Cliente",
            Celular = "987654321",
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(usuario, new ValidationContext(usuario), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Email")));
    }

    [TestMethod]
    public void Usuario_CelularExcede9Caracteres_FallaValidacion()
    {
        // Preparar
        var usuario = new Usuario
        {
            Nombres = "Carlos",
            Apellidos = "Lopez",
            Email = "carlos@mail.com",
            Contrasenia = "clave123",
            Rol = "Cliente",
            Celular = "9876543210",   // 10 chars, máximo es 9
            FechaRegistro = DateTime.Now,
            Estado = true
        };

        // Actuar
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(usuario, new ValidationContext(usuario), resultados, true);

        // Assert
        Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Celular")));
    }

    // AuthService.HashPassword 

    [TestMethod]
    public void HashPassword_RetornaHashDistintoAlTextoOriginal()
    {
        // Preparar
        string password = "miPassword123";

        // Actuar
        string hash = AuthService.HashPassword(password);

        // Assert
        Assert.AreNotEqual(password, hash);
    }

    [TestMethod]
    public void HashPassword_HashEsVerificableConBCrypt()
    {
        // Preparar
        string password = "miPassword123";

        // Actuar
        string hash = AuthService.HashPassword(password);
        bool esValido = BCrypt.Net.BCrypt.Verify(password, hash);

        // Assert
        Assert.IsTrue(esValido);
    }

    [TestMethod]
    public void HashPassword_MismaPassword_GeneraHashesDiferentes()
    {
        // Preparar
        string password = "miPassword123";

        // Actuar
        string hash1 = AuthService.HashPassword(password);
        string hash2 = AuthService.HashPassword(password);

        // Assert
        Assert.AreNotEqual(hash1, hash2);  
    }


    [TestMethod]
    public async Task ValidarCredenciales_CredencialesCorrectas_RetornaUsuario()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("VC_Ok").Options;
        using var db = new AppDbContext(opciones);
        db.Usuarios.Add(new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Perez",
            Email = "juan@test.com",
            Contrasenia = AuthService.HashPassword("pass123"),
            Rol = "Cliente",
            Celular = "999999999",
            FechaRegistro = DateTime.Now,
            Estado = true
        });
        await db.SaveChangesAsync();
        var service = new AuthService(db);

        // Actuar
        var resultado = await service.ValidarCredencialesAsync("juan@test.com", "pass123");

        // Assert
        Assert.IsNotNull(resultado);
        Assert.AreEqual("juan@test.com", resultado.Email);
    }

    [TestMethod]
    public async Task ValidarCredenciales_PasswordIncorrecta_RetornaNull()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("VC_PassMal").Options;
        using var db = new AppDbContext(opciones);
        db.Usuarios.Add(new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Perez",
            Email = "juan2@test.com",
            Contrasenia = AuthService.HashPassword("pass123"),
            Rol = "Cliente",
            Celular = "999999999",
            FechaRegistro = DateTime.Now,
            Estado = true
        });
        await db.SaveChangesAsync();
        var service = new AuthService(db);

        // Actuar
        var resultado = await service.ValidarCredencialesAsync("juan2@test.com", "passwordMala");

        // Assert
        Assert.IsNull(resultado);
    }

    [TestMethod]
    public async Task ValidarCredenciales_UsuarioInactivo_RetornaNull()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("VC_Inactivo").Options;
        using var db = new AppDbContext(opciones);
        db.Usuarios.Add(new Usuario
        {
            Nombres = "Juan",
            Apellidos = "Perez",
            Email = "inactivo@test.com",
            Contrasenia = AuthService.HashPassword("pass123"),
            Rol = "Cliente",
            Celular = "999999999",
            FechaRegistro = DateTime.Now,
            Estado = false   // deshabilitado
        });
        await db.SaveChangesAsync();
        var service = new AuthService(db);

        // Actuar
        var resultado = await service.ValidarCredencialesAsync("inactivo@test.com", "pass123");

        // Assert
        Assert.IsNull(resultado);
    }


    [TestMethod]
    public async Task RegistrarCliente_EmailNuevo_RetornaTrue()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("RC_Ok").Options;
        using var db = new AppDbContext(opciones);
        var service = new AuthService(db);
        var vm = new RegistroViewModel
        {
            Nombres = "Ana",
            Apellidos = "Lopez",
            Email = "ana@test.com",
            Contrasenia = "clave123",
            Celular = "988888888",
            Direccion = "Av. Lima 1",
            Ciudad = "Lima",
            CodigoPostal = "15001",
            DniRuc = "12345678",
            FechaNacimiento = new DateTime(1995, 5, 10)
        };

        // Actuar
        bool resultado = await service.RegistrarClienteAsync(vm);

        // Assert
        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public async Task RegistrarCliente_EmailDuplicado_RetornaFalse()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("RC_Dup").Options;
        using var db = new AppDbContext(opciones);
        db.Usuarios.Add(new Usuario
        {
            Nombres = "Ana",
            Apellidos = "Lopez",
            Email = "ana@test.com",
            Contrasenia = AuthService.HashPassword("clave123"),
            Rol = "Cliente",
            Celular = "987654321",
            FechaRegistro = DateTime.Now,
            Estado = true
        });
        await db.SaveChangesAsync();
        var service = new AuthService(db);
        var vm = new RegistroViewModel
        {
            Nombres = "Ana",
            Apellidos = "Lopez",
            Email = "ana@test.com",
            Contrasenia = "otraClave",
            Direccion = "Jr. Cusco 55",
            Ciudad = "Cusco",
            CodigoPostal = "08001",
            DniRuc = "87654321",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };

        // Actuar
        bool resultado = await service.RegistrarClienteAsync(vm);

        // Assert
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public async Task RegistrarCliente_GuardaPerfilClienteAsociado()
    {
        // Preparar
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("RC_Perfil").Options;
        using var db = new AppDbContext(opciones);
        var service = new AuthService(db);
        var vm = new RegistroViewModel
        {
            Nombres = "Carlos",
            Apellidos = "Rios",
            Email = "carlos@test.com",
            Contrasenia = "clave456",
            Celular = "988888888",
            Direccion = "Calle Falsa 123",
            Ciudad = "Cajamarca",
            CodigoPostal = "06001",
            DniRuc = "11223344",
            FechaNacimiento = new DateTime(1988, 3, 20)
        };

        // Actuar
        await service.RegistrarClienteAsync(vm);

        // Assert
        Assert.AreEqual(1, db.Clientes.Count());
        Assert.AreEqual("Cajamarca", db.Clientes.First().Ciudad);
    }
}
