using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class OrdenPantoneDto
    {
        public int IdFormula { get; set; }
        public required string NombreColor { get; set; }
        public MezclaResultadoDto MezclaPrueba { get; set; } = null!;
        public MezclaResultadoDto MezclaOrden { get; set; } = null!;
    }
}