using System;

namespace Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public int IdRol { get; set; }
        public int PrimerInicio { get; set; }

        public Rol Rol { get; set; }
        public ICollection<OrdenImpresion> OrdenImpresions { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}