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
        public required string Lote { get; set; }
        public required string Nombre { get; set; }
        public required string Fabricante { get; set; }
        public required decimal Costo { get; set; }
        public string Presentacion { get; set; }
        public ICollection<TintaBase> TintasBase { get; set; }
    }
}