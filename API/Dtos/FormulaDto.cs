using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class FormulaDto
    {
        public int Id { get; set; }
        public required string NombreColor { get; set; }
        public ICollection<DetalleFormula> DetalleFormulas { get; set; }
    }
}