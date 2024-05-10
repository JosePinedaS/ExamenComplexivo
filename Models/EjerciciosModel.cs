using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Salud_Vida_2.Models
{
    [Table("ejercicios")]
    public class EjerciciosModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEjercicio { get; set; }

        [DisplayName("Nombre del Ejercicio")]
        [StringLength(20)]
        public string NombreEjercicio { get; set; }

        [DisplayName("Zona afectada del cuerpo")]
        [StringLength(50)]
        public string ZonaEjercicio { get; set; }

        [DisplayName("Valor MET")]
        public double MetEjercicio { get; set; }

        [DisplayName("Detalle del Ejercicio")]
        [StringLength(300)]
        public string DetalleEjercicio { get; set; }

        [DisplayName("Imagen del Ejercicio")]
        [StringLength(200)]
        public string ImagenEjercicio { get; set; }

        [DisplayName("Estado del Ejercicio")]
        public Boolean EstadoEjercicio { get; set; }
    }
}
