using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Salud_Vida_2.Models
{
    public class MyContextDB : DbContext
    {
        public MyContextDB() : base("name=MySQLConnectionString")
        {
        }

        public DbSet<UserModel> usuario { get; set; }
        public DbSet<EjerciciosModel> ejercicios { get; set; }
        public DbSet<RegistroActividadModel> registro_actividad { get; set; }
        public DbSet<RegistroEjercicioModel> registro_ejercicio { get; set; }

    }

}