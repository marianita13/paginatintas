
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
    }
}