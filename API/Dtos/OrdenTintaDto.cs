using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class OrdenTintaDto
    {
        public int IdTinta { get; set; }
        public string NombreTinta { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal GramosCalculados { get; set; }
    }
}