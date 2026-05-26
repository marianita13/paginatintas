using System;

namespace Domain.Entities
{
    public class Logotipo : BaseEntity
    {
        public string NombreLogo { get; set; }
        public string UrlImagen { get; set; }
        public int IdEmpresa { get; set; }
        public int IdFormula { get; set; }


        public Empresa Empresa { get; set; }
        public Formula FormulaPrincipal { get; set; }
        public ICollection<LogotipoColor> LogotipoColors { get; set; }
        public ICollection<OrdenImpresion> OrdenImpresions { get; set; }
    }
}