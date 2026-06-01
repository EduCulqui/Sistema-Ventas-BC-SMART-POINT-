using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Controllers;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public class TestAdministrador
    {

        [TestMethod]
        public async Task Dashboard_Get_RetornaVista()
        {
            // Preparar
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ADM_Dashboard").Options;
            using var db = new AppDbContext(opciones);
            var controller = new AdminController(db);

            // Actuar
            var resultado = await controller.Dashboard();

            // Assert
            Assert.IsInstanceOfType(resultado, typeof(Microsoft.AspNetCore.Mvc.ViewResult));
        }

        [TestMethod]
        public async Task Dashboard_CuentaProductosActivos()
        {
            // Preparar
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ADM_ProdCount").Options;
            using var db = new AppDbContext(opciones);

            var cat = new Categoria { NomCategoria = "Mac", Descripcion = "desc", Estado = true };
            var prov = new Proveedor { NombreEmpresa = "Apple Inc.", ContactoNombre = "Luis", Celular = "999999999", Email = "t@t.com", Direccion = "Lima", Ruc = "20123456789", Estado = true };
            db.Categorias.Add(cat);
            db.Proveedores.Add(prov);
            await db.SaveChangesAsync();

            db.Productos.Add(new Producto { Nombre = "MacBook Pro", Descripcion = "desc", Precio = 2000m, StockActual = 5, StockMinimo = 1, CategoriaId = cat.IdCategoria, ProveedorId = prov.IdProveedor, Estado = true });
            db.Productos.Add(new Producto { Nombre = "Magic Mouse", Descripcion = "desc", Precio = 80m, StockActual = 10, StockMinimo = 2, CategoriaId = cat.IdCategoria, ProveedorId = prov.IdProveedor, Estado = false });
            await db.SaveChangesAsync();

            // Actuar
            int totalActivos = await db.Productos.CountAsync(p => p.Estado);

            // Assert
            Assert.AreEqual(1, totalActivos);   // solo el activo
        }

        // Validaciones del modelo Administrador 

        [TestMethod]
        public void Administrador_DatosValidos_PasaValidacion()
        {
            // Preparar
            var admin = new Administrador
            {
                Cargo = "Gerente de Ventas",
                FechaContrato = new DateTime(2024, 1, 15),
                UsuarioId = 1
            };

            // Actuar
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(admin, new ValidationContext(admin), resultados, true);

            // Assert
            Assert.AreEqual(0, resultados.Count);
        }

        [TestMethod]
        public void Administrador_SinCargo_FallaValidacion()
        {
            // Preparar
            var admin = new Administrador
            {
                Cargo = "",                    // campo requerido vacío
                FechaContrato = DateTime.Now,
                UsuarioId = 1
            };

            // Actuar
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(admin, new ValidationContext(admin), resultados, true);

            // Assert
            Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Cargo")));
        }

        [TestMethod]
        public void Administrador_CargoExcede100Caracteres_FallaValidacion()
        {
            // Preparar
            var admin = new Administrador
            {
                Cargo = new string('G', 101),  // 101 chars, máximo es 100
                FechaContrato = DateTime.Now,
                UsuarioId = 1
            };

            // Actuar
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(admin, new ValidationContext(admin), resultados, true);

            // Assert
            Assert.IsTrue(resultados.Any(r => r.MemberNames.Contains("Cargo")));
        }

        // ── Lógica de estado de venta ─────────────────────────────────────────────

        [TestMethod]
        public async Task ValidarPago_AprobadoActualizaEstadoAPagado()
        {
            // Preparar
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ADM_Pago").Options;
            using var db = new AppDbContext(opciones);
            db.Ventas.Add(new Venta
            {
                FechaVenta = DateTime.Now,
                SubtotalSinDescuento = 1000m,
                IGV = 180m,
                TotalVenta = 1180m,
                MetodoPago = "Yape",
                EstadoPago = "En verificación",
                ClienteId = 1
            });
            await db.SaveChangesAsync();
            var venta = db.Ventas.First();

            // Actuar
            venta.EstadoPago = "Pagado";
            await db.SaveChangesAsync();
            var resultado = await db.Ventas.FindAsync(venta.IdVenta);

            // Assert
            Assert.AreEqual("Pagado", resultado!.EstadoPago);
        }

        [TestMethod]
        public async Task ValidarPago_RechazadoActualizaEstadoARechazado()
        {
            // Preparar
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ADM_Rechazo").Options;
            using var db = new AppDbContext(opciones);
            db.Ventas.Add(new Venta
            {
                FechaVenta = DateTime.Now,
                SubtotalSinDescuento = 500m,
                IGV = 90m,
                TotalVenta = 590m,
                MetodoPago = "Transferencia",
                EstadoPago = "En verificación",
                ClienteId = 1
            });
            await db.SaveChangesAsync();
            var venta = db.Ventas.First();

            // Actuar
            venta.EstadoPago = "Rechazado";
            await db.SaveChangesAsync();
            var resultado = await db.Ventas.FindAsync(venta.IdVenta);

            // Assert
            Assert.AreEqual("Rechazado", resultado!.EstadoPago);
        }

    }
}
