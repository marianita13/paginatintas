using System;

namespace Domain.Entities
{
    public class OrdenImpresion : BaseEntity
    {
        public int IdUsuario { get; set; }
        public int IdLogotipo { get; set; }
        public string FechaOrden { get; set; }
        public decimal VolumenTotal { get; set; }

        public Usuario Usuario { get; set; }
        public Logotipo Logotipo { get; set; }
    }
}