using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sistema_BC_SMART_POINT.Models.ViewModels;
using Sistema_BC_SMART_POINT.Services;
using System.Security.Claims;

namespace Sistema_BC_SMART_POINT.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) => _auth = auth;

        // GET Auth para Login
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST 
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _auth.ValidarCredencialesAsync(vm.Email, vm.Contrasenia);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(vm);
            }

            // Crear claims para la cookie de sesión
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name,  $"{usuario.Nombres} {usuario.Apellidos}"),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role,  usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = vm.Recordarme });

            // Regresar a la URL de origen o al catálogo
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Catalogo");
        }

        // GET Auth para Registro
        public IActionResult Registro() => View(new RegistroViewModel());

        // POST 
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            bool ok = await _auth.RegistrarClienteAsync(vm);
            if (!ok)
            {
                ModelState.AddModelError("Email", "Este correo ya está registrado.");
                return View(vm);
            }

            TempData["Exito"] = "Cuenta creada. Ahora inicia sesión.";
            return RedirectToAction("Login");
        }

        // GET Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Catalogo");
        }
    }
}
