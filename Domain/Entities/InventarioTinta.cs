using System;

namespace Domain.Entities
{
    public class InventarioTinta : BaseEntity
    {
        public int? IdTintaBase { get; set; }
        public required string IdInterno { get; set; }
        public required string Lote { get; set; }
        public required string Nombre { get; set; }
        public required string Fabricante { get; set; }
        public required string Presentacion { get; set; }
        public required decimal Costo { get; set; }
        public TintaBase TintaBase { get; set; }
    }
}