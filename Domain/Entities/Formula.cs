using System;

namespace Domain.Entities
{
    public class Formula : BaseEntity
    {
        public int? IdEmpresa { get; set; }
        public required string NombreColor { get; set; }
        public Empresa Empresa { get; set; }
        public required ICollection<LogotipoColor> LogotipoColors { get; set; }
        public required ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}