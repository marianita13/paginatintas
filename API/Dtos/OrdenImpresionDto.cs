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
        public string FechaOrden { get; set; }
        public decimal VolumenTotal { get; set; }
    }
}