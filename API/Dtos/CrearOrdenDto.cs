using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class CrearOrdenDto
    {
        public required string NumeroOrden { get; set; }
        public int NumeroCajas { get; set; }
        public int PruebaColor { get; set; }
        public List<int> IdsFormulas { get; set; } = new();
    }
}