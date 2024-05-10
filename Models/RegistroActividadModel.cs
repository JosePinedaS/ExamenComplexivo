using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Salud_Vida_2.Models
{
    [Table("registroactividad")]
    public class RegistroActividadModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRegistroAct { get; set; }

        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public UserModel Usuario { get; set; }

        [Column(TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan HoraInicio { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan HoraFinal { get; set; }

        [Required(ErrorMessage = "El campo Peso Inicial es requerido.")]
        [Display(Name = "Peso Inicial")]
        public double PesoInicial { get; set; }

        public double PesoFinal { get; set; }

        public double CaloriasQuemadas { get; set; }
    }
}