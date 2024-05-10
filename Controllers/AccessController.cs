using Salud_Vida_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace Salud_Vida_2.Controllers
{
    public class AccessController : Controller
    {
        private readonly AuthenticationService _authenticationService;

        public AccessController()
        {
            _authenticationService = new AuthenticationService();
        }

        private MyContextDB db = new MyContextDB();
        // GET: Access
        public ActionResult Login()
        {
            return View(new UserModel());  // Inicializa un nuevo modelo para la vista
        }

        // POST: Access/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel model)  // Utiliza el modelo para mantener los datos
        { 
            if (ModelState.IsValid)  // Verifica que el modelo sea válido
            {
                // Hashea la contraseña ingresada por el usuario
                string hashedPassword = HashPassword(model.Password);

                var user = db.usuario.FirstOrDefault(u => u.Usuario == model.Usuario && u.Password == hashedPassword/*model.Password*/);
                if (user != null)
                { 
                    if (user.EstadoUsuario)
                    {
                        // Usuario activo, procede con el inicio de sesión
                        //Guarda los datos
                        Session["UserID"] = user.IdUsuario;
                        Session["Rol"] = user.TipoUsuario;
                        Session["NombreCompleto"] = user.PrimerNombre + " " + user.PrimerApellido;

                        // Verifica si el usuario no es administrador (TipoUsuario = 2)
                        Session["EsNoAdministrador"] = false;
                        Session["EsNoAdministrador"] = (user.TipoUsuario == 2);

                        // Calcular la diferencia entre la fecha final y la fecha actual
                        TimeSpan diferencia = user.FechaFinal - DateTime.Now;
                        // Guardar la diferencia en días en la sesión
                        Session["DiasRestantes"] = diferencia.Days;
                        switch (user.TipoUsuario)
                        {
                            case 0: // Jefe
                            case 1:  // Administrador

                                return RedirectToAction("ListUser", "Admin");
                            case 2:  // Usuario
                                return RedirectToAction("Index", "User");  // Página de inicio para usuarios comunes

                            default:
                                return RedirectToAction("Index", "Home");  // Redirección predeterminada
                        }

                    }
                    else
                    {
                        // Usuario no está activo, muestra un mensaje de error
                        ViewBag.Error = "Usuario inhabilitado contacte con un administrador";
                        return View(model); // Asegúrate de regresar a la vista de inicio de sesión
                    }
                }
            }

            ViewBag.Error = "Nombre de usuario o contraseña incorrecta.";
            return View(model);  // Devuelve el modelo a la vista para mantener los datos
        }

        // GET: Access/Logout
        public ActionResult Logout()
        {
            Session.Clear();  // Limpia la sesión
            return RedirectToAction("Login", "Access");  // Asegúrate de redirigir correctamente.
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // ComputeHash retorna un byte array
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convertimos los bytes hasheados a string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Filtro de autorización personalizado
        public class CustomAuthorizeAttribute : AuthorizeAttribute
        {
            public CustomAuthorizeAttribute(params int[] allowedRoles)
            {
                AllowedRoles = allowedRoles;
            }
            public int[] AllowedRoles { get; }
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var userRole = (int?)httpContext.Session["Rol"];
                if (userRole.HasValue && AllowedRoles.Contains(userRole.Value))
                {
                    // El usuario tiene uno de los roles permitidos
                    return true;
                }
                // El usuario no tiene permiso para acceder a esta acción
                return false;
            }
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                // Redirigir al Home/Index con un mensaje de alerta
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index",
                    alertMessage = "Permisos insuficientes. Acceda primero al sistema."
                }));
            }
        }

        // Servicio de autenticación
        public class AuthenticationService
        {
            public bool Authenticate(string username, string password)
            {
                // Lógica para autenticar al usuario
                // Verificar las credenciales en la base de datos u otro origen de datos
                // Retorna true si las credenciales son válidas, de lo contrario false
                return false;
            }

            public void SetSessionData(string username)
            {
                // Establecer datos en la sesión después de la autenticación
            }

            public void ClearSession()
            {
                // Limpiar la sesión después del cierre de sesión
            }
        }
    }
}
