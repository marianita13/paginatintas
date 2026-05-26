using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace API.Dtos
{
    public class LogotipoDto
    {
        public int Id { get; set; }
        public string NombreLogo { get; set; }
        public string UrlImagen { get; set; }
    }
}