using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_BC_SMART_POINT.Migrations
{
    /// <inheritdoc />
    public partial class Mig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomCategoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.IdCategoria);
                });

            migrationBuilder.CreateTable(
                name: "CuponDescuento",
                columns: table => new
                {
                    IdCuponDescuento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoCupon = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreación = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuponDescuento", x => x.IdCuponDescuento);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    IdProveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactoNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ruc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.IdProveedor);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contrasenia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Capacidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.IdProducto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Administradores",
                columns: table => new
                {
                    IdAdministrador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaContrato = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administradores", x => x.IdAdministrador);
                    table.ForeignKey(
                        name: "FK_Administradores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DniRuc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IdCliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    IdVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubtotalSinDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGV = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CuponDescuentoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.IdVenta);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ventas_CuponDescuento_CuponDescuentoId",
                        column: x => x.CuponDescuentoId,
                        principalTable: "CuponDescuento",
                        principalColumn: "IdCuponDescuento",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVentas",
                columns: table => new
                {
                    IdDetalleVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVentas", x => x.IdDetalleVenta);
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "IdProducto");
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Envios",
                columns: table => new
                {
                    IdEnvio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DireccionEnvio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoEnvio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmpresaTransporte = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroSeguimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    AdministradorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envios", x => x.IdEnvio);
                    table.ForeignKey(
                        name: "FK_Envios_Administradores_AdministradorId",
                        column: x => x.AdministradorId,
                        principalTable: "Administradores",
                        principalColumn: "IdAdministrador");
                    table.ForeignKey(
                        name: "FK_Envios_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Administradores_UsuarioId",
                table: "Administradores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuponDescuento_CodigoCupon",
                table: "CuponDescuento",
                column: "CodigoCupon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentas_ProductoId",
                table: "DetallesVentas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentas_VentaId",
                table: "DetallesVentas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_AdministradorId",
                table: "Envios",
                column: "AdministradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_VentaId",
                table: "Envios",
                column: "VentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuponDescuentoId",
                table: "Ventas",
                column: "CuponDescuentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesVentas");

            migrationBuilder.DropTable(
                name: "Envios");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Administradores");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "CuponDescuento");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
