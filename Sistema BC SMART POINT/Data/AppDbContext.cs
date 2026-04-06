using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Models;

namespace Sistema_BC_SMART_POINT.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<AlertaStock> AlertaStock { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<DetalleVenta> DetallesVentas { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoVariante> ProductoVariantes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<CuponDescuento> CuponDescuento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario 1:0..1 Cliente
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.UsuarioId);

            // Usuario 1:0..1 Administrador
            modelBuilder.Entity<Administrador>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.Administrador)
                .HasForeignKey<Administrador>(a => a.UsuarioId);

            // Categoria 1:N Producto
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId);

            // Proveedor 1:N Producto
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId);

            // Cliente 1:N Venta
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId);

            // Venta 1:N DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(dv => dv.VentaId);

            // Producto 1:N DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);

            // Venta 1:0..1 Envio
            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Venta)
                .WithOne(v => v.Envio)
                .HasForeignKey<Envio>(e => e.VentaId);

            // Administrador 1:N Envio
            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Administrador)
                .WithMany(a => a.Envios)
                .HasForeignKey(e => e.AdministradorId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProductoVariante 1:N AlertaStock
            modelBuilder.Entity<AlertaStock>()
                .HasOne(a => a.ProductoVariante)
                .WithMany()
                .HasForeignKey(a => a.ProductoVarianteId);

            // Producto 1:N ProductoVariante
            modelBuilder.Entity<ProductoVariante>()
                .HasOne(pv => pv.Producto)
                .WithMany(p => p.Variantes)
                .HasForeignKey(pv => pv.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar duplicados
            modelBuilder.Entity<ProductoVariante>()
                .HasIndex(pv => new { pv.ProductoId})
                .IsUnique();

            // CuponDescuento 1:N Venta (un cupon puede usarse en multiples ventas)
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.CuponDescuento)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.CuponDescuentoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indice unico para CodigoCupon
            modelBuilder.Entity<CuponDescuento>()
                .HasIndex(c => c.CodigoCupon)
                .IsUnique();
        }
    }
}
