using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class OrdenImpresionDto
    {
        public int Id { get; set; }
        public required string NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public decimal VolumenTotal { get; set; }
        public bool Estado { get; set; }
        public decimal CostoTotal { get; set; }
        public int NumeroCajas { get; set; }
        public int PruebaColor { get; set; }
        public ICollection<Formula> Formulas { get; set; }
        public List<OrdenPantoneDto> Pantones { get; set; } = new();
    }

    public class ActualizarCajasDto
    {
        public int PruebaColor { get; set; }  // N° cajas impresas con la prueba de color
        public int NumeroCajas { get; set; }  // N° cajas totales de la orden
        public decimal CostoTotal { get; set; } // Costo total de la orden
    }
}