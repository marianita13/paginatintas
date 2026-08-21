using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class InventarioTintaDto
    {
        public int Id { get; set; }
        public required string IdInterno { get; set; }
        public int IdTintaBase { get; set; }
        public required string Lote { get; set; }
        public required string Nombre { get; set; }
        public required string Fabricante { get; set; }
        public required string Proveedor {get; set;}
        public required decimal Costo { get; set; }
        public required decimal Presentacion { get; set; }
    }
}