using System;

namespace Domain.Entities
{
    public class DetalleFormula : BaseEntity
    {
        public int IdFormula { get; set; }
        public int IdTinta { get; set; }
        public decimal Porcentaje { get; set; }

        public Formula Formula { get; set; }
        public TintaBase TintaBase { get; set; }
    }
}