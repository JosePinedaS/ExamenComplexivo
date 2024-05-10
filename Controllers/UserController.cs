using Org.BouncyCastle.Crypto.Generators;
using Salud_Vida_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Google.Protobuf.WellKnownTypes;
using K4os.Compression.LZ4.Internal;
using Org.BouncyCastle.Utilities;
using System.Security.Cryptography;
using ZstdSharp.Unsafe;
using System.Data;
using System.Globalization;
using System.Text;
using static Salud_Vida_2.Controllers.AccessController;

namespace Salud_Vida_2.Controllers
{
    [CustomAuthorize(0, 1, 2)]
    public class UserController : Controller
    {
        private readonly MyContextDB _context;

        public UserController()
        {
            _context = new MyContextDB();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Ejercicio()
        {
            var ejercicio = _context.ejercicios.ToList();
            return View(ejercicio);
        }
        public ActionResult CambioPass()
        {
            return View();
        }
        public ActionResult Progreso()
        {
            return View();
        }
        public ActionResult Entrenamiento()
        {
            return View();
        }
        public ActionResult RegistroEjercicio()
        {
            var registrosEjercicio = _context.ejercicios.ToList();
            return View(registrosEjercicio);
        }
        public ActionResult Usuario()
        {
            // Obtener el ID de usuario de la sesión
            int? userId = Session["UserID"] as int?;
            if (userId.HasValue)
            {
                // Buscar el usuario en la base de datos
                var usuario = _context.usuario.FirstOrDefault(u => u.IdUsuario == userId);
                if (usuario != null)
                {
                    // Llenar el modelo UserModel con los datos del usuario encontrado
                    var userModel = new UserModel
                    {
                        PrimerNombre = usuario.PrimerNombre,
                        SegundoNombre = usuario.SegundoNombre,
                        PrimerApellido = usuario.PrimerApellido,
                        SegundoApellido = usuario.SegundoApellido,
                        FechaNacimiento = usuario.FechaNacimiento,
                        Estatura = usuario.Estatura,
                        Peso = usuario.Peso,
                        Genero = usuario.Genero,
                        Direccion = usuario.Direccion,
                        Correo = usuario.Correo,
                        Telefono = usuario.Telefono,
                        Usuario = usuario.Usuario,
                    };
                    return View(userModel);
                }
            }

            // Si no se encuentra el usuario o no hay ID en la sesión, redirigir a una página de error o hacer otra acción adecuada
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Usuario(UserModel usuarioModificado)
        {
            int? userId = Session["UserID"] as int?;
            if (!userId.HasValue)
            {
                // Manejo cuando el ID del usuario no está disponible en la sesión
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Usuario no identificado");
            }

            if (ModelState.IsValid)
            {
                var usuarioExistente = _context.usuario.Find(userId.Value);
                if (usuarioExistente == null)
                {
                    // Manejo cuando no se encuentra el usuario en la base de datos
                    return HttpNotFound("Usuario no encontrado");
                }

                // Actualiza los valores del usuario existente con los valores del usuario modificado
                usuarioExistente.PrimerNombre = usuarioModificado.PrimerNombre;
                usuarioExistente.SegundoNombre = usuarioModificado.SegundoNombre;
                usuarioExistente.PrimerApellido = usuarioModificado.PrimerApellido;
                usuarioExistente.SegundoApellido = usuarioModificado.SegundoApellido;
                usuarioExistente.FechaNacimiento = usuarioModificado.FechaNacimiento;
                usuarioExistente.Estatura = usuarioModificado.Estatura;
                usuarioExistente.Genero = usuarioModificado.Genero;
                usuarioExistente.Peso = usuarioModificado.Peso;
                usuarioExistente.Direccion = usuarioModificado.Direccion;
                usuarioExistente.Correo = usuarioModificado.Correo;
                usuarioExistente.Telefono = usuarioModificado.Telefono;
                usuarioExistente.Usuario = usuarioModificado.Usuario;

                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Ejercicio", "User");
                }
                catch (Exception)
                {
                    // Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "No se pudo guardar los cambios. Intente de nuevo, y si el problema persiste, vea el log del sistema.");
                }
            }

            // Si hay errores de validación, vuelve a mostrar el formulario con los datos ingresados
            return View(usuarioModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambioPass(string currentPassword, string newPassword, string confirmNewPassword)
        {
            int? userId = Session["UserID"] as int?;
            if (!userId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Usuario no identificado");
            }

            var user = _context.usuario.Find(userId);
            if (user == null)
            {
                return HttpNotFound("Usuario no encontrado");
            }

            // Verifica que la contraseña actual sea correcta
            if (user.Password != currentPassword)
            {
                ModelState.AddModelError("", "La contraseña actual no es correcta.");
                return View();
            }

            // Verifica que las nuevas contraseñas coincidan
            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("", "Las nuevas contraseñas no coinciden.");
                return View();
            }

            // Actualiza la contraseña en la base de datos
            string hashedPassword = HashPassword(newPassword);
            user.Password = hashedPassword;
            _context.SaveChanges();

            // Establece un mensaje TempData para mostrar que la contraseña se ha actualizado con éxito
            TempData["SuccessMessage"] = "La contraseña se ha actualizado con éxito.";

            return RedirectToAction("Usuario", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Entrenamiento(RegistroActividadModel model)
        {
            if (ModelState.IsValid)
            {
                int? userId = Session["UserID"] as int?;
                if (!userId.HasValue)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Usuario no identificado");
                }

                // Guarda el peso inicial en una variable de sesión
                Session["PesoInicial"] = model.PesoInicial;

                // Crea un nuevo registro de actividad
                var registro = new RegistroActividadModel
                {
                    IdUsuario = userId.Value,
                    Fecha = DateTime.Now,
                    HoraInicio = DateTime.Now.TimeOfDay,
                    HoraFinal = new TimeSpan(0, 0, 0), // Hora final se inicializa en cero
                    PesoInicial = model.PesoInicial,
                    PesoFinal = 0, // Peso final se inicializa en cero
                    CaloriasQuemadas = 0 // Calorías quemadas se inicializan en cero
                };

                _context.registro_actividad.Add(registro);
                _context.SaveChanges();

                // Guarda el ID del registro recién creado en una variable de sesión
                Session["RegistroActividadId"] = registro.IdRegistroAct;
                Session["PesoInicial"] = registro.PesoInicial;

                return RedirectToAction("RegistroEjercicio", "User"); // Redirige a la vista de índice o a cualquier otra vista adecuada
            }

            // Si el modelo no es válido, vuelve a mostrar la vista con los datos del modelo
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistroEjercicio(RegistroEjercicioModel model)
        {
            if (ModelState.IsValid)
            {

                int IdEjercici = model.IdEjercicio;
                // Obtener el ID del registro de actividad de la sesión
                int registroActividadId = Convert.ToInt32(Session["RegistroActividadID"]);

                // Calcular las calorías quemadas
                double pesoInicial = Convert.ToDouble(Session["PesoInicial"]);
                double caloriasQuemadas = CalcularCaloriasQuemadas(model.TiempoEjercicio, pesoInicial, model.IdEjercicio);

                // Crear una nueva instancia de RegistroEjercicioModel
                var registroEjercicio = new RegistroEjercicioModel
                {
                    IdRegistroActividad = registroActividadId,
                    IdEjercicio = model.IdEjercicio,
                    TiempoEjercicio = model.TiempoEjercicio,
                    RepeticionesEjercicio = model.RepeticionesEjercicio,
                    SeriesEjercicio = model.SeriesEjercicio,
                    CaloriasEjercicio = caloriasQuemadas
                };

                // Agregar el registro a la base de datos y guardar los cambios
                _context.registro_ejercicio.Add(registroEjercicio);
                _context.SaveChanges();

                // Redireccionar a una vista de confirmación u otra vista apropiada
                return RedirectToAction("RegistroEjercicio", "User");
            }

            // Si hay errores en el modelo, volver a mostrar el formulario
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminarEntrenamiento(double pesoFinal)
        {
            if (pesoFinal <= 0)
            {
                ModelState.AddModelError("pesoFinal", "El campo de peso final es obligatorio.");
                return View();
            }
            // Obtener el ID del registro de actividad de la sesión
            int registroActividadId = Convert.ToInt32(Session["RegistroActividadID"]);
            // Obtener el registro de actividad correspondiente
            var registroActividad = _context.registro_actividad.Find(registroActividadId);

            if (registroActividad == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Actividad no identificada");
            }

            // Obtener el peso inicial de la variable de sesión
            double pesoInicial = Convert.ToDouble(Session["PesoInicial"]);
            // Pasar el peso inicial al formulario como un campo oculto
            ViewBag.PesoInicial = pesoInicial;

            // Actualizar los campos necesarios
            registroActividad.HoraFinal = DateTime.Now.TimeOfDay; // Obtener la hora actual del ordenador
            registroActividad.PesoFinal = pesoFinal;

            // Calcular las calorías quemadas sumando las calorías de todos los ejercicios
            double totalCaloriasQuemadas = 0;

            try
            {
                totalCaloriasQuemadas = _context.registro_ejercicio
                    .Where(re => re.IdRegistroActividad == registroActividadId)
                    .Sum(re => re.CaloriasEjercicio);
            }
            catch (InvalidOperationException)
            {
                // Manejar la excepción en caso de que CaloriasEjercicio sea nulo
                // Asignar un valor predeterminado de 0 a totalCaloriasQuemadas
                totalCaloriasQuemadas = 0;
            }



            registroActividad.CaloriasQuemadas = totalCaloriasQuemadas;

            // Guardar los cambios en la base de datos
            _context.SaveChanges();

            // Redirigir a la página de inicio u otra página apropiada

            return RedirectToAction("Ejercicio", "User");
        }

        [HttpPost]
        public JsonResult CaloriasSemanales()
        {
            int? userId = Session["UserID"] as int?;


            // Filtrar los registros de actividad por el ID de usuario y agrupar por fecha, sumando las calorías quemadas
            var registros = _context.registro_actividad
                .Where(ra => ra.IdUsuario == userId)
                .GroupBy(ra => ra.Fecha)
                .Select(grupo => new
                {
                    Fecha = grupo.Key,
                    CaloriasQuemadas = grupo.Sum(ra => ra.CaloriasQuemadas)
                })
                .ToList();

            return Json(registros, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CaloriasMensuales()
        {
            int? userId = Session["UserID"] as int?;

            // Obtener la fecha actual y retroceder 6 meses
            DateTime fechaInicio = DateTime.Now.Date.AddMonths(-6);
            DateTime fechaFin = DateTime.Now.Date;

            // Filtrar los registros de actividad por el ID de usuario y por el rango de fechas
            var registros1 = _context.registro_actividad
    .Where(ra => ra.IdUsuario == userId && ra.Fecha >= fechaInicio && ra.Fecha <= fechaFin)
    .GroupBy(ra => new { ra.Fecha.Year, ra.Fecha.Month }) // Agrupar por año y mes
    .ToList() // Ejecutar la consulta para traer los datos de la base de datos
    .Select(grupo => new
    {
        Mes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(grupo.Key.Month), // Obtener el mes del grupo
        CaloriasQuemadas = grupo.Sum(ra => ra.CaloriasQuemadas)
    })
    .ToList();




            return Json(registros1, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult EjerciciosComunes()
        {
            int? userId = Session["UserID"] as int?;

            // Obtener la fecha actual y retroceder 6 meses
            DateTime fechaInicio = DateTime.Now.Date.AddMonths(-6);
            DateTime fechaFin = DateTime.Now.Date;

            // Filtrar los registros de actividad por el ID de usuario y por el rango de fechas
            var ejerciciosUsuario = _context.registro_actividad
     .Where(ra => ra.IdUsuario == userId)
     .Join(_context.registro_ejercicio, ra => ra.IdRegistroAct, re => re.IdRegistroActividad, (ra, re) => new { ra, re })
     .Join(_context.ejercicios, x => x.re.IdEjercicio, e => e.IdEjercicio, (x, e) => new { x, e })
     .GroupBy(x => new { IdEjercicio = x.e.IdEjercicio, NombreEjercicio = x.e.NombreEjercicio })
     .Select(g => new
     {
         IdEjercicio = g.Key.IdEjercicio,
         NombreEjercicio = g.Key.NombreEjercicio,
         Cantidad = g.Count()
     })
     .OrderByDescending(x => x.Cantidad) // Ordenar por la cantidad de veces que se ha realizado cada ejercicio
     .Take(5) // Tomar los primeros 5 elementos
     .ToList();


            return Json(ejerciciosUsuario, JsonRequestBehavior.AllowGet);
        }
        private double CalcularCaloriasQuemadas(int tiempoEjercicio, double pesoInicial, int idEjercicio)
        {
            // Obtener el objeto EjercicioModel correspondiente al ID proporcionado
            var ejercicio = _context.ejercicios.Find(idEjercicio);

            // Verificar si se encontró el ejercicio
            if (ejercicio != null)
            {
                // Calcular las calorías quemadas usando el MET del ejercicio
                double metEjercicio = ejercicio.MetEjercicio;
                double caloriasQuemadas = metEjercicio * pesoInicial * (tiempoEjercicio / 60.0);
                return caloriasQuemadas;
            }
            else
            {
                // Manejar el caso en que no se encuentra el ejercicio
                throw new Exception("No se encontró el ejercicio con el ID proporcionado.");
            }
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}