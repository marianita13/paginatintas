using System;

namespace Domain.Entities
{
    public class Empresa : BaseEntity
    {
        public string NombreComercial { get; set; }
        public string Nit { get; set; }
        public string Telefono { get; set; }
        public string CorreoContacto { get; set; }
        public string FechaRegistro { get; set; }
        public ICollection<Formula> Formulas { get; set; }
        public ICollection<Logotipo> Logotipos { get; set; }
    }
}