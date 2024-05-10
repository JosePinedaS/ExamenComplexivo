using Salud_Vida_2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Salud_Vida_2.Controllers.AccessController;

namespace Salud_Vida_2.Controllers
{
    [CustomAuthorize(0, 1)]
    public class AdminController : Controller
    {
        // GET: Admin
        private MyContextDB db = new MyContextDB();

        //Obtencio del Tipo de Usuario que es
        public class AuthorizeRoleAttribute : AuthorizeAttribute
        {
            private readonly int[] allowedRoles;
            public AuthorizeRoleAttribute(params int[] roles)
            {
                this.allowedRoles = roles;
            }

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var isAuthorized = base.AuthorizeCore(httpContext);
                if (!isAuthorized)
                {
                    return false;
                }

                int? rolId = httpContext.Session["Rol"] as int?;
                if (rolId != null && allowedRoles.Contains(rolId.Value))
                {
                    return true;
                }

                return false;
            }
        }

        //=============================================================================USUARIO==============================================================================
        //Funcion de restriccion de Tipos de Usuario
        //Funcion Agregar y Modificar Usuarios
        public ActionResult RegisterOrEditUser(int? id)
        {
            int? rolId = Session["Rol"] as int?;
            if (rolId != 0 && rolId != 1)
            {
                return RedirectToAction("Index", "Home");  // O mostrar una vista de "Acceso Denegado"
            }
            UserModel model;
            if (id.HasValue)
            {
                model = db.usuario.Find(id.Value);
                if (model == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Title = "Editar Usuario";
            }
            else
            {
                model = new UserModel();
                ViewBag.Title = "Registrar Usuario";
            }
            // Recuperar la contraseña del usuario si existe
            var userPassword = db.usuario.Where(u => u.IdUsuario == id).Select(u => u.Password).FirstOrDefault();
            if (!string.IsNullOrEmpty(userPassword))
            {
                // Asignar la contraseña al modelo para evitar enviar un valor nulo
                model.Password = userPassword;
            }
            return View(model); // Utiliza la misma vista para editar o registrar
        }

        //Funcion de agregar o aplicar los cambios realizados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterOrEditUser(UserModel model)
        {
            if (Session["Rol"] != null && (int)Session["Rol"] == 1) // Si es Administrador
            {
                model.TipoUsuario = 2;  // Forzar el rol a Usuario
            }

            if (ModelState.IsValid)
            {
                if (model.IdUsuario > 0) // Edición de usuario existente
                {
                    ViewBag.Title = "Editar Usuario";
                    // Verificar si el usuario desea cambiar la contraseña
                    if (string.IsNullOrEmpty(model.NewPassword))
                    {
                        // Si no se proporciona una nueva contraseña, mantener la contraseña original
                        model.Password = db.usuario.Where(u => u.IdUsuario == model.IdUsuario).Select(u => u.Password).FirstOrDefault();
                    }
                    else
                    {
                        // Si se proporciona una nueva contraseña, usarla
                        model.Password = HashPassword(model.NewPassword);
                    }
                    // Verificar si el tipo de usuario es Usuario (TipoUsuario = 2) y si la fecha final ha pasado
                    if (model.TipoUsuario == 2 && model.FechaFinal < DateTime.Now)
                    {
                        model.EstadoUsuario = false; // Estado inactivo
                    }
                    else
                    {
                        model.EstadoUsuario = true; // Estado activo
                    }
                    // Verificar si la fecha final ha pasado y actualizar el estado del usuario en consecuencia
                    if (model.FechaFinal < DateTime.Now)
                    {
                        model.EstadoUsuario = false; // Estado inactivo
                    }
                    else
                    {
                        model.EstadoUsuario = true; // Estado activo
                    }

                    // Asume que la entidad ya está siendo rastreada por el contexto de EF
                    db.Entry(model).State = EntityState.Modified;
                }
                else // Registro de nuevo usuario
                {
                    ViewBag.Title = "Registrar Usuario";
                    // Asegúrate de hashear la contraseña antes de guardarla
                    model.Password = HashPassword(model.Password);
                    // Verificar si el tipo de usuario es Usuario (TipoUsuario = 2) y si la fecha final ha pasado
                    if (model.TipoUsuario == 2 && model.FechaFinal < DateTime.Now)
                    {
                        model.EstadoUsuario = false; // Estado inactivo
                    }
                    else
                    {
                        model.EstadoUsuario = true; // Estado activo
                    }
                    db.usuario.Add(model);
                }
                try
                {
                    db.SaveChanges();
                    // Establecer el mensaje de éxito para registro o edición de usuario
                    if (model.IdUsuario > 0)
                    {
                        if (model.EstadoUsuario)
                        {
                            Session["SuccessMessageEdit"] = "Usuario editado correctamente (habilitado).";
                        }
                        else
                        {
                            Session["SuccessMessageEdit"] = "Usuario editado correctamente (inhabilitado).";
                        }
                    }
                    else
                    {
                        if (model.EstadoUsuario)
                        {
                            Session["SuccessMessageRegister"] = "Usuario registrado correctamente (habilitado).";
                        }
                        else
                        {
                            Session["SuccessMessageRegister"] = "Usuario registrado correctamente (inhabilitado).";
                        }
                    }
                    return RedirectToAction("ListUser"); // Redirige a la lista de usuarios después de guardar
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el usuario: " + ex.Message);
                }
            }

            return View(model);
        }


        //Funcion de Listar los Usuarios
        //Funcion de Buscar los Usuarios segun el nombre, apellido o usuario
        public ActionResult ListUser(string search = "")
        {
            int? rolId = Session["Rol"] as int?;
            if (rolId != 0 && rolId != 1)
            {
                return RedirectToAction("Index", "Home");  // Redirige si no es Jefe o Admin
            }

            var usuarios = db.usuario
                            .Where(u => (string.IsNullOrEmpty(search) ||
                                         u.PrimerNombre.Contains(search) ||
                                         u.SegundoNombre.Contains(search) ||
                                         u.PrimerApellido.Contains(search) ||
                                         u.SegundoApellido.Contains(search) ||
                                         u.Usuario.Contains(search)) &&
                                        u.TipoUsuario != 0)
                            .ToList();

            ViewBag.CurrentFilter = search;
            return View(usuarios);
        }

        //Funciones de habilitacion y deshabilitacion de los Usuarios
        public ActionResult DisableUser(int id)
        {
            var usuario = db.usuario.Find(id);
            if (usuario != null)
            {
                usuario.EstadoUsuario = false;  // Deshabilita el usuario
                db.SaveChanges();
                return RedirectToAction("ListUser");
            }
            return HttpNotFound();
        }
        public ActionResult EnableUser(int id)
        {
            var usuario = db.usuario.Find(id);
            if (usuario != null)
            {
                usuario.EstadoUsuario = true;  // Deshabilita el usuario
                db.SaveChanges();
                return RedirectToAction("ListUser");
            }
            return HttpNotFound();
        }

        //Funcion de cambiar la contrasena del Usuario
        [HttpPost]
        public ActionResult CambiarContrasena(UserModel user, string newPassword, int id)
        {
            // Verificar si el usuario y la nueva contraseña no son nulos
            if (user != null && !string.IsNullOrEmpty(newPassword))
            {
                // Buscar al usuario por su Id
                var usuario = db.usuario.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound(); // Manejar el caso en que no se encuentre el usuario
                }

                // Hash de la nueva contraseña
                usuario.Password = HashPassword(newPassword);

                // Actualizar la contraseña en la base de datos
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                // Redirigir al usuario al editar usuario del mismo usuario que estaba editando
                return RedirectToAction("RegisterOrEditUser", new { id });
            }
            else
            {
                // Manejar el caso en que el usuario o la nueva contraseña sean nulos
                // Aquí puedes mostrar un mensaje de error o realizar alguna otra acción apropiada
                ModelState.AddModelError("", "Usuario o nueva contraseña inválidos");
                return View(user);
            }
        }

        //Funcion de encriptacion de la contrasena
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

        //=============================================================================EJERCICIOS============================================================================

        //Funcion de restriccion de Tipos de usuario ---- Solo acceso a Jefe y Admin
        public ActionResult RegisterOrEditExercice(int? id)
        {
            int? rolId = Session["Rol"] as int?;
            if (rolId != 0 && rolId != 1)
            {
                return RedirectToAction("Index", "Home");  // O mostrar una vista de "Acceso Denegado"
            }

            EjerciciosModel model;
            if (id.HasValue)
            {
                model = db.ejercicios.Find(id.Value);
                if (model == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Title = "Editar Ejercicio";
            }
            else
            {
                model = new EjerciciosModel();
                ViewBag.Title = "Registrar Ejercicio";
            }

            return View(model); // Utiliza la misma vista para editar o registrar
        }

        //Funcion de agregar o aplicar los cambios realizados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterOrEditExercice(EjerciciosModel modele, UserModel modelu)
        {
            if (Session["Rol"] != null && (int)Session["Rol"] == 1) // Si es Administrador
            {
                modelu.TipoUsuario = 2;  // Forzar el rol a Usuario
            }

            if (ModelState.IsValid)
            {
                if (modele.IdEjercicio > 0) // Edición de ejercicio existente
                {
                    // Asume que la entidad ya está siendo rastreada por el contexto de EF
                    db.Entry(modele).State = EntityState.Modified;
                }
                else // Registro de nuevo ejercicio
                {
                    // Agrega el nuevo ejercicio
                    db.ejercicios.Add(modele);
                }
                try
                {
                    db.SaveChanges();
                    // Establecer el mensaje de éxito para registro o edición de usuario
                    if (modele.IdEjercicio > 0)
                    {
                        Session["SuccessMessageEdit"] = "Usuario editado correctamente.";
                    }
                    else
                    {
                        Session["SuccessMessageRegister"] = "Usuario registrado correctamente.";
                    }
                    return RedirectToAction("ListExercise"); // Redirige a la lista de usuarios después de guardar
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el ejercicio: " + ex.Message);
                }
            }

            return View(modele);
        }

        //Funcion de Listar los Usuarios
        //Funcion de Buscar los Usuarios segun el nombre, apellido o usuario
        public ActionResult ListExercise(string search = "")
        {
            int? rolId = Session["Rol"] as int?;
            if (rolId != 0 && rolId != 1)
            {
                return RedirectToAction("Index", "Home");  // Redirige si no es Jefe o Admin
            }

            var ejercicios = db.ejercicios.Where(
                e => string.IsNullOrEmpty(search) ||
                e.NombreEjercicio.Contains(search) ||
                e.ZonaEjercicio.Contains(search)).ToList();

            ViewBag.CurrentFilter = search;
            return View(ejercicios);
        }

        //Funciones de habilitacion y deshabilitacion de los Usuarios
        public ActionResult DisableExercise(int id)
        {
            var ejercicio = db.ejercicios.Find(id);
            if (ejercicio != null)
            {
                ejercicio.EstadoEjercicio = false;  // Deshabilita el usuario
                db.SaveChanges();
                return RedirectToAction("ListExercise");
            }
            return HttpNotFound();
        }
        public ActionResult EnablExercise(int id)
        {
            var ejercicio = db.ejercicios.Find(id);
            if (ejercicio != null)
            {
                ejercicio.EstadoEjercicio = true;  // Deshabilita el usuario
                db.SaveChanges();
                return RedirectToAction("ListExercise");
            }
            return HttpNotFound();
        }
    }
}