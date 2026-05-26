using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class EmpresaDto
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; }
        public string Nit {get; set;}
        public string Telefono {get; set;}
        public string CorreoContacto {get; set;}
        public string FechaOrden { get; set; }
        public ICollection<Logotipo> Logotipos { get; set; }
        public ICollection<Formula> Formulas { get; set; }
    }
}