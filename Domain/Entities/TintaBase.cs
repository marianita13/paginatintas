using System;

namespace Domain.Entities
{
    public class TintaBase : BaseEntity
    {
        public required string NombreTinta { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo_alerta { get; set; }
        public decimal PrecioUnitario { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
        public ICollection<InventarioTinta> InventarioTintas { get; set; }
    }
}