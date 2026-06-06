using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;

namespace API.Services
{
    public interface IMezclaService
    {
        Task<MezclaResultadoDto> CalcularMezclaAsync(MezclaPeticionDto request);
        Task<MezclaResultadoDto> ConfirmarMezclaAsync(MezclaPeticionDto request);
    }
}