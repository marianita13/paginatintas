using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class OrdenImpresion : BaseEntity
    {
        public int IdUsuario { get; set; }
        public int NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public decimal VolumenTotal { get; set; }

        [Column(TypeName = "tinyint(1)")]
        public bool Estado { get; set; }

        public decimal CostoTotal { get; set; }
        public int NumeroCajas { get; set; }
        public int PruebaColor { get; set; }

        public Usuario Usuario { get; set; }
    }
}