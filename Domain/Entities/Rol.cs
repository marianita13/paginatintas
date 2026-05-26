using System;

namespace Domain.Entities
{
    public class Rol : BaseEntity
    {
        public string Nombre { get; set; }
        public ICollection<Usuario> Usuarios { get; set; }
    }
}