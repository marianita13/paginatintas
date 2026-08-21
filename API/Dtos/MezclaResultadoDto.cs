namespace API.Dtos
{
    public class MezclaResultadoDto
    {
        public int IdFormula { get; set; }
        public required string NombreColor { get; set; }
        public decimal PesoTotalGramos { get; set; }
        public decimal SumaPorcentajes { get; set; }  // debe ser 1.00
        public bool PorcentajesValidos { get; set; }  // true si suma == 1
        public List<MezclaTintaDto> Tintas { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }

    public class MezclaTintaDto
    {
        public int IdTinta { get; set; }
        public required string NombreTinta { get; set; }
        public decimal Porcentaje { get; set; }        // ej: 0.0875
        public decimal PorcentajeDisplay { get; set; } // ej: 8.75 (para mostrar)
        public decimal GramosNecesarios { get; set; }  // ej: 437.5g
        public decimal StockActual { get; set; }
        public bool StockSuficiente { get; set; }
        public decimal PrecioUnitario { get; set; } 
    }
}