using System;
using System.Threading.Tasks;
using API.Dtos;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MezclaController : BaseController
    {
        private readonly IMezclaService _mezclaService;

        public MezclaController(IMezclaService mezclaService)
        {
            _mezclaService = mezclaService;
        }

        /// <summary>
        /// Calcula los gramos de cada tinta base sin descontar el stock.
        /// Usar para previsualizar antes de confirmar.
        /// </summary>
        [HttpPost("calcular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MezclaResultadoDto>> Calcular([FromBody] MezclaPeticionDto request)
        {
            try
            {
                if (request.PesoTotalGramos <= 0)
                    return BadRequest("El peso total debe ser mayor a 0 gramos.");

                var resultado = await _mezclaService.CalcularMezclaAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Confirma la mezcla y descuenta el stock de cada tinta base.
        /// Solo procede si hay stock suficiente y los porcentajes suman 1.
        /// </summary>
        [HttpPost("confirmar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MezclaResultadoDto>> Confirmar([FromBody] MezclaPeticionDto request)
        {
            try
            {
                if (request.PesoTotalGramos <= 0)
                    return BadRequest("El peso total debe ser mayor a 0 gramos.");

                var resultado = await _mezclaService.ConfirmarMezclaAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
