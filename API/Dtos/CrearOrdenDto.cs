using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class CrearOrdenDto
    {
        public int IdUsuario { get; set; }
        public required string NumeroOrden { get; set; }
        public int NumeroCajas { get; set; }    
        public int PruebaColor { get; set; }
        public string MedidaLamina { get; set; } = string.Empty;
        public string MontajeImpresion { get; set; } = string.Empty;
        public List<int> IdsFormulas { get; set; } = new();
    }
}