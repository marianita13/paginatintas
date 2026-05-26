using System;

namespace Domain.Entities
{
    public class Formula : BaseEntity
    {
        public int IdEmpresa { get; set; }
        public int NombreColor { get; set; }
        
        public Empresa Empresa { get; set; }
        public ICollection<LogotipoColor> LogotipoColors { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}