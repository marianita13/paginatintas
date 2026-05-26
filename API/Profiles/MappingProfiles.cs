using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;

namespace API.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<DetalleFormula, DetalleFormulaDto>().ReverseMap();
            CreateMap<Empresa, EmpresaDto>().ReverseMap();
            CreateMap<Formula, FormulaDto>().ReverseMap();
            CreateMap<LogotipoColor, LogotipoColorDto>().ReverseMap();
            CreateMap<Logotipo, LogotipoDto>().ReverseMap();
            CreateMap<OrdenImpresion, OrdenImpresionDto>().ReverseMap();
            CreateMap<Rol, RolDto>().ReverseMap();
            CreateMap<TintaBase, TintaBaseDto>().ReverseMap();
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
        }
    }
}