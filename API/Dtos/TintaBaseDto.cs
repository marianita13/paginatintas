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
        public string NombreTinta { get; set; }
        public string CodigoHex { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo_alerta { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}