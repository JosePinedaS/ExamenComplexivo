using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Salud_Vida_2.Models
{
    [Table("registroejercicios")]
    public class RegistroEjercicioModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRegistroEjercicio { get; set; }

        [ForeignKey("RegistroActividad")]
        public int IdRegistroActividad { get; set; }
        public RegistroActividadModel RegistroActividad { get; set; }

        [ForeignKey("Ejercicio")]
        public int IdEjercicio { get; set; }
        public EjerciciosModel Ejercicio { get; set; }

        public int TiempoEjercicio { get; set; }

        public int RepeticionesEjercicio { get; set; }

        public int SeriesEjercicio { get; set; }

        public double CaloriasEjercicio { get; set; }
    }
}