using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class OrdenImpresionController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrdenImpresionController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // GET /api/OrdenImpresion
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrdenImpresionDto>>> Get()
    {
        var entidades = await _unitOfWork.OrdenImpresions.GetAllAsync();
        return Ok(_mapper.Map<List<OrdenImpresionDto>>(entidades));
    }

    // GET /api/OrdenImpresion/{id} — detalle enriquecido con pantones y mezclas calculadas
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenImpresionDto>> Get(int id)
    {
        var orden = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
        if (orden == null) return NotFound();

        var ordenFormulas = _unitOfWork.OrdenFormulas
            .Find(of => of.IdOrdenImpresion == id)
            .ToList();

        var detalle = new OrdenImpresionDto
        {
            Id           = orden.Id,
            NumeroOrden  = orden.NumeroOrden,
            FechaOrden   = orden.FechaOrden,
            Estado       = orden.Estado,
            CostoTotal   = orden.CostoTotal,
            NumeroCajas  = orden.NumeroCajas,
            PruebaColor  = orden.PruebaColor,
            VolumenTotal = orden.VolumenTotal
        };

        foreach (var of in ordenFormulas)
        {
            var formula = await _unitOfWork.Formulas.GetFormulaConDetallesAsync(of.IdFormula);
            if (formula == null) continue;

            decimal sumaPorcentajes = formula.DetalleFormulas.Sum(d => d.Porcentaje);
            bool porcentajesValidos = Math.Round(sumaPorcentajes, 2) == 1.00m;

            var mezclaPrueba = new MezclaResultadoDto
            {
                IdFormula          = formula.Id,
                NombreColor        = formula.NombreColor,
                PesoTotalGramos    = 100m,
                SumaPorcentajes    = sumaPorcentajes,
                PorcentajesValidos = porcentajesValidos
            };

            var mezclaOrden = new MezclaResultadoDto
            {
                IdFormula          = formula.Id,
                NombreColor        = formula.NombreColor,
                SumaPorcentajes    = sumaPorcentajes,
                PorcentajesValidos = porcentajesValidos
            };

            decimal totalGramosOrden = 0;

            foreach (var d in formula.DetalleFormulas)
            {
                decimal gramosPrueba = Math.Round(d.Porcentaje * 100m, 2);

                mezclaPrueba.Tintas.Add(new MezclaTintaDto
                {
                    IdTinta           = d.TintaBase.Id,
                    NombreTinta       = d.TintaBase.NombreTinta,
                    Porcentaje        = d.Porcentaje,
                    PorcentajeDisplay = d.Porcentaje * 100m,
                    GramosNecesarios  = gramosPrueba,
                    StockActual       = d.TintaBase.StockActual,
                    StockSuficiente   = d.TintaBase.StockActual >= gramosPrueba,
                    PrecioUnitario    = d.TintaBase.PrecioUnitario   // ← NUEVO
                });

                decimal gramosOrden = orden.PruebaColor > 0
                    ? Math.Round((orden.NumeroCajas * gramosPrueba) / orden.PruebaColor, 0)
                    : 0;

                totalGramosOrden += gramosOrden;

                bool tieneStockOrden = d.TintaBase.StockActual >= gramosOrden;
                if (!tieneStockOrden)
                    mezclaOrden.Advertencias.Add(
                        $"Stock insuficiente de {d.TintaBase.NombreTinta}. " +
                        $"Se requieren {gramosOrden}g y hay {d.TintaBase.StockActual}g.");

                mezclaOrden.Tintas.Add(new MezclaTintaDto
                {
                    IdTinta           = d.TintaBase.Id,
                    NombreTinta       = d.TintaBase.NombreTinta,
                    Porcentaje        = d.Porcentaje,
                    PorcentajeDisplay = d.Porcentaje * 100m,
                    GramosNecesarios  = gramosOrden,
                    StockActual       = d.TintaBase.StockActual,
                    StockSuficiente   = tieneStockOrden,
                    PrecioUnitario    = d.TintaBase.PrecioUnitario   // ← NUEVO
                });
            }

            mezclaOrden.PesoTotalGramos = totalGramosOrden;

            detalle.Pantones.Add(new OrdenPantoneDto
            {
                IdFormula    = formula.Id,
                NombreColor  = formula.NombreColor,
                MezclaPrueba = mezclaPrueba,
                MezclaOrden  = mezclaOrden
            });
        }

        return Ok(detalle);
    }

    // POST /api/OrdenImpresion — crea orden con sus fórmulas
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrdenImpresionDto>> Post([FromBody] CrearOrdenDto dto)
    {
        if (dto.IdsFormulas == null || !dto.IdsFormulas.Any())
            return BadRequest("Debes seleccionar al menos un Pantone.");

        var orden = new OrdenImpresion
        {
            IdUsuario    = 2, // TODO: User.FindFirstValue(ClaimTypes.NameIdentifier)
            NumeroOrden  = dto.NumeroOrden,
            FechaOrden   = DateTime.UtcNow,
            VolumenTotal = 0,
            Estado       = false,
            CostoTotal   = 0,
            NumeroCajas  = dto.NumeroCajas,
            PruebaColor  = dto.PruebaColor
        };

        _unitOfWork.OrdenImpresions.Add(orden);
        await _unitOfWork.SaveAsync();

        foreach (var idFormula in dto.IdsFormulas)
        {
            _unitOfWork.OrdenFormulas.Add(new OrdenFormula
            {
                IdFormula        = idFormula,
                IdOrdenImpresion = orden.Id
            });
        }

        await _unitOfWork.SaveAsync();
        return CreatedAtAction(nameof(Get), new { id = orden.Id }, new { id = orden.Id });
    }

    // PUT /api/OrdenImpresion/{id}/cajas — actualiza cajas de prueba y cajas de orden
    [HttpPut("{id}/cajas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActualizarCajas(int id, [FromBody] ActualizarCajasDto dto)
    {
        var orden = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
        if (orden == null) return NotFound();
        orden.PruebaColor = dto.PruebaColor;
        orden.NumeroCajas = dto.NumeroCajas;
        orden.CostoTotal = dto.CostoTotal;
        _unitOfWork.OrdenImpresions.Update(orden);
        await _unitOfWork.SaveAsync();
        return Ok(new { mensaje = "Cajas actualizadas." });
    }

    // PUT /api/OrdenImpresion/{id}/estado — cambia estado 0↔1
    [HttpPut("{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CambiarEstado(int id, [FromBody] bool nuevoEstado)
    {
        var orden = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
        if (orden == null) return NotFound();
        orden.Estado = nuevoEstado;
        _unitOfWork.OrdenImpresions.Update(orden);
        await _unitOfWork.SaveAsync();
        return Ok(new { mensaje = "Estado actualizado." });
    }

    // DELETE /api/OrdenImpresion/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var orden = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
        if (orden == null) return NotFound();
        _unitOfWork.OrdenImpresions.Remove(orden);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}