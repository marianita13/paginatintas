using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class OrdenImpresionDto
    {
        public int Id { get; set; }
        public int NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public decimal VolumenTotal { get; set; }
        public bool Estado { get; set; }
        public decimal CostoTotal { get; set; }
        public int NumeroCajas { get; set; }
        public int PruebaColor { get; set; }
        public ICollection<Formula> Formulas { get; set; }
    }
}