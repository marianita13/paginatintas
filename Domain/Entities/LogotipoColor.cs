using System;

namespace Domain.Entities
{
    public class LogotipoColor : BaseEntity
    {
        public int IdFormula { get; set; }
        public int IdLogotipo { get; set; }
        public int EsColorprincipal { get; set; }

        public Logotipo Logotipo { get; set; }
        public Formula Formula { get; set; }
    }
}