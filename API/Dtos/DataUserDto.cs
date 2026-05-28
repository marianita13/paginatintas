using System;

namespace API.Dtos
{
    public class DataUserDto
    {
        public string Mensaje { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}