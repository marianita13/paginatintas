using System;

namespace Domain.Entities
{
    public class TintaBase : BaseEntity
    {
        public string NombreTinta { get; set; }
        public string CodigoHex { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo_alerta { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}