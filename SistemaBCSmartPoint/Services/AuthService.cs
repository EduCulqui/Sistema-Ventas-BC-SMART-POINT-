using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Models;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Sistema_BC_SMART_POINT.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db) => _db = db;

        // BCrypt
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<Usuario?> ValidarCredencialesAsync(string email, string password)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Estado);

            if (usuario == null)
                return null;

            bool esValido = BCrypt.Net.BCrypt.Verify(password, usuario.Contrasenia);

            return esValido ? usuario : null;
        }

        public async Task<bool> RegistrarClienteAsync(RegistroViewModel vm)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == vm.Email))
                return false; // Email ya existe

            var usuario = new Usuario
            {
                Nombres = vm.Nombres,
                Apellidos = vm.Apellidos,
                Email = vm.Email,
                Contrasenia = HashPassword(vm.Contrasenia),
                Rol = "Cliente",
                Celular = vm.Celular,
                FechaRegistro = DateTime.Now,
                Estado = true
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            // Crear el perfil de cliente asociado
            _db.Clientes.Add(new Cliente
            {
                UsuarioId = usuario.IdUsuario,
                Direccion = vm.Direccion,
                Ciudad = vm.Ciudad,
                CodigoPostal = vm.CodigoPostal,
                DniRuc = vm.DniRuc,
                FechaNacimiento = vm.FechaNacimiento
            });
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
