using System;

namespace Domain.Entities
{
    public class OrdenFormula : BaseEntity
    {
        public int IdFormula { get; set; }
        public int IdOrdenImpresion { get; set; }

        public Formula Formula { get; set; }
        public OrdenImpresion OrdenImpresion { get; set; }

    }
}