using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salud_Vida_2.Models
{
    [Table("usuarios")]
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("ID de Usuario")]
        public int IdUsuario { get; set; }

        [StringLength(15)]
        [DisplayName("Primer Nombre")]
        public string PrimerNombre { get; set; }

        [StringLength(15)]
        [DisplayName("Segundo Nombre")]
        public string SegundoNombre { get; set; }

        [StringLength(15)]
        [DisplayName("Primer Apellido")]
        public string PrimerApellido { get; set; }

        [StringLength(15)]
        [DisplayName("Segundo Apellido")]
        public string SegundoApellido { get; set; }

        [DisplayName("Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [DisplayName("Estatura")]
        public int Estatura { get; set; }

        [StringLength(10)]
        [DisplayName("Género")]
        public string Genero { get; set; }

        [DisplayName("Peso")]
        public double Peso { get; set; }

        [StringLength(150)]
        [DisplayName("Dirección")]
        public string Direccion { get; set; }

        [StringLength(100)]
        [DisplayName("Correo Electrónico")]
        public string Correo { get; set; }

        [StringLength(10)]
        [DisplayName("Teléfono")]
        public string Telefono { get; set; }

        [DisplayName("Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [DisplayName("Fecha Final")]
        public DateTime FechaFinal { get; set; }

        [DisplayName("Estado del Usuario")]
        public Boolean EstadoUsuario { get; set; }

        [DisplayName("Tipo de Usuario")]
        public int TipoUsuario { get; set; }

        [StringLength(10)]
        [DisplayName("Usuario")]
        public string Usuario { get; set; }

        [StringLength(200)]
        [DisplayName("Contraseña")]
        public string Password { get; set; }

        [NotMapped]
        [DisplayName("Nueva Contraseña")]
        public string NewPassword { get; set; }

    }
}
