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
            CreateMap<OrdenImpresion, OrdenImpresionDto>().ReverseMap();
            CreateMap<Rol, RolDto>().ReverseMap();
            CreateMap<TintaBase, TintaBaseDto>().ReverseMap();
            CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.IdRol));
            CreateMap<UsuarioDto, Usuario>()
            .ForMember(dest => dest.IdRol, opt => opt.MapFrom(src => src.Rol));
            CreateMap<InventarioTinta, InventarioTintaDto>().ReverseMap();
            CreateMap<OrdenFormula, OrdenFormulaDto>().ReverseMap();
        }
    }
}