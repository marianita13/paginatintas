using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class TintaBaseDto
    {
        public int Id { get; set; }
        public required string NombreTinta { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo_alerta { get; set; }
        public decimal PrecioUnitario { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}