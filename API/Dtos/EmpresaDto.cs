using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class EmpresaDto
    {
        public int Id { get; set; }
        [Required]
        public required string NombreComercial { get; set; }
        public string Telefono {get; set;}
        public DateTime FechaOrden { get; set; }
        public required string Información { get; set; }
        public ICollection<Formula> Formulas { get; set; }
    }
}