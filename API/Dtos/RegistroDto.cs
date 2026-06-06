using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class RegistroDto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; }

        [Required]
        public int IdRol {get; set;}
    }
}
