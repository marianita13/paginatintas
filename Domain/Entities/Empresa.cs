using System;

namespace Domain.Entities
{
    public class Empresa : BaseEntity
    {
        public string NombreComercial { get; set; }
        public string Telefono { get; set; }
        public string FechaRegistro { get; set; }
        public string Información { get; set; }
        public ICollection<Formula> Formulas { get; set; }
    }
}