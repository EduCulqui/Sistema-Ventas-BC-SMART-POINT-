using Microsoft.AspNetCore.Http;
using Moq;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]

    public class TestCarrito
    {
        private static ISession CrearSession()
        {
            var store = new Dictionary<string, byte[]>();
            var mock = new Mock<ISession>();

            // Mockear TryGetValue (método base de GetString)
            mock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string k, out byte[] v) =>
                {
                    var found = store.TryGetValue(k, out var val);
                    v = val;
                    return found;
                });

            // Mockear Set (método base de SetString)
            mock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback((string k, byte[] v) => store[k] = v);

            // Mockear Remove
            mock.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback((string k) => store.Remove(k));

            return mock.Object;
        }

        // ObtenerCarrito 

        [TestMethod]
        public void ObtenerCarrito_SesionVacia_RetornaListaVacia()
        {
            var session = CrearSession();
            var service = new CarritoService();

            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(0, carrito.Count);
        }

        // AgregarItem 

        [TestMethod]
        public void AgregarItem_ProductoNuevo_SeAgregaAlCarrito()
        {
            var session = CrearSession();
            var service = new CarritoService();
            var item = new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 1 };

            service.AgregarItem(session, item);
            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(1, carrito.Count);
        }

        [TestMethod]
        public void AgregarItem_ProductoExistente_IncrementaCantidad()
        {
            var session = CrearSession();
            var service = new CarritoService();
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 1 });

            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 3 });
            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(4, carrito[0].Cantidad);   // 1 + 3
        }

        [TestMethod]
        public void AgregarItem_ProductosDiferentes_AmbosPersistenEnCarrito()
        {
            var session = CrearSession();
            var service = new CarritoService();

            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 1 });
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 2, Nombre = "Magic Mouse", PrecioUnitario = 80m, Cantidad = 2 });
            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(2, carrito.Count);
        }

        // QuitarItem 

        [TestMethod]
        public void QuitarItem_ProductoExistente_EliminaItemDelCarrito()
        {
            var session = CrearSession();
            var service = new CarritoService();
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 5, Nombre = "iPad Air", PrecioUnitario = 1000m, Cantidad = 1 });

            service.QuitarItem(session, productoId: 5);
            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(0, carrito.Count);
        }

        [TestMethod]
        public void QuitarItem_ProductoInexistente_NoLanzaExcepcion()
        {
            // Preparar
            var session = CrearSession();
            var service = new CarritoService();
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 1 });

            // Actuar y Assert
            try
            {
                service.QuitarItem(session, productoId: 99);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Se lanzó una excepción inesperada: {ex.Message}");
            }
        }

        // Limpiar 

        [TestMethod]
        public void Limpiar_ConItems_DejaCarritoVacio()
        {
            var session = CrearSession();
            var service = new CarritoService();
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 1 });
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 2, Nombre = "Magic Mouse", PrecioUnitario = 80m, Cantidad = 1 });

            service.Limpiar(session);
            var carrito = service.ObtenerCarrito(session);

            Assert.AreEqual(0, carrito.Count);
        }

        // ContarItems 

        [TestMethod]
        public void ContarItems_VariosProductos_SumaTodasLasCantidades()
        {
            var session = CrearSession();
            var service = new CarritoService();
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 1, Nombre = "MacBook Pro", PrecioUnitario = 2000m, Cantidad = 3 });
            service.AgregarItem(session, new CarritoItemViewModel { ProductoId = 2, Nombre = "Magic Mouse", PrecioUnitario = 80m, Cantidad = 2 });

            int total = service.ContarItems(session);

            Assert.AreEqual(5, total);
        }

        [TestMethod]
        public void ContarItems_CarritoVacio_RetornaCero()
        {
            var session = CrearSession();
            var service = new CarritoService();

            int total = service.ContarItems(session);

            Assert.AreEqual(0, total);
        }

        // CarritoItemViewModel.SubTotal 

        [TestMethod]
        public void CarritoItem_SubTotal_EsPrecioUnitarioPorCantidad()
        {
            var item = new CarritoItemViewModel
            {
                ProductoId = 1,
                Nombre = "Magic Keyboard",
                PrecioUnitario = 150m,
                Cantidad = 3
            };

            decimal subtotal = item.SubTotal;

            Assert.AreEqual(450m, subtotal);
        }

    }
}
