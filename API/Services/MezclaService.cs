using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using Domain.Interfaces;

namespace API.Services
{
    public class MezclaService : IMezclaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MezclaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MezclaResultadoDto> CalcularMezclaAsync(MezclaPeticionDto request)
        {
            // 1. Obtener la fórmula con todos sus detalles y tintas
            var formula = await _unitOfWork.Formulas.GetFormulaConDetallesAsync(request.IdFormula);

            if (formula == null)
                throw new Exception($"No se encontró la fórmula con Id {request.IdFormula}.");

            if (formula.DetalleFormulas == null || !formula.DetalleFormulas.Any())
                throw new Exception($"La fórmula '{formula.NombreColor}' no tiene tintas base asignadas.");

            var resultado = new MezclaResultadoDto
            {
                IdFormula = formula.Id,
                NombreColor = formula.NombreColor,
                PesoTotalGramos = request.PesoTotalGramos
            };

            // 2. Calcular suma total de porcentajes (debe ser exactamente 1.00)
            decimal sumaPorcentajes = formula.DetalleFormulas.Sum(d => d.Porcentaje);
            resultado.SumaPorcentajes = Math.Round(sumaPorcentajes, 4);
            resultado.PorcentajesValidos = Math.Abs(sumaPorcentajes - 1m) < 0.0001m;

            if (!resultado.PorcentajesValidos)
                resultado.Advertencias.Add(
                    $"⚠️ Los porcentajes suman {sumaPorcentajes:P2} en lugar de 100%. " +
                    $"Revisa la fórmula antes de proceder.");

            // 3. Calcular gramos de cada tinta y verificar stock
            foreach (var detalle in formula.DetalleFormulas)
            {
                var tinta = detalle.TintaBase;
                decimal gramosNecesarios = Math.Round(detalle.Porcentaje * request.PesoTotalGramos, 2);
                bool stockSuficiente = tinta.StockActual >= gramosNecesarios;

                resultado.Tintas.Add(new MezclaTintaDto
                {
                    IdTinta          = tinta.Id,
                    NombreTinta      = tinta.NombreTinta,
                    CodigoHex        = tinta.CodigoHex,
                    Porcentaje       = detalle.Porcentaje,
                    PorcentajeDisplay = Math.Round(detalle.Porcentaje * 100, 4),
                    GramosNecesarios = gramosNecesarios,
                    StockActual      = tinta.StockActual,
                    StockSuficiente  = stockSuficiente
                });

                if (!stockSuficiente)
                    resultado.Advertencias.Add(
                        $"⚠️ Stock insuficiente para '{tinta.NombreTinta}': " +
                        $"necesitas {gramosNecesarios}g pero hay {tinta.StockActual}g disponibles.");
            }

            // 4. Ordenar tintas de mayor a menor cantidad (la mayoritaria primero)
            resultado.Tintas = resultado.Tintas
                .OrderByDescending(t => t.GramosNecesarios)
                .ToList();

            return resultado;
        }

        public async Task<MezclaResultadoDto> ConfirmarMezclaAsync(MezclaPeticionDto request)
        {
            // 1. Calcular primero para validar todo
            var resultado = await CalcularMezclaAsync(request);

            // 2. Verificar que todos los porcentajes sean válidos antes de descontar
            if (!resultado.PorcentajesValidos)
                throw new Exception("No se puede confirmar: los porcentajes no suman 1 (100%).");

            // 3. Verificar que haya stock suficiente para TODAS las tintas
            var sinStock = resultado.Tintas.Where(t => !t.StockSuficiente).ToList();
            if (sinStock.Any())
            {
                var nombres = string.Join(", ", sinStock.Select(t => t.NombreTinta));
                throw new Exception($"No se puede confirmar: stock insuficiente para: {nombres}.");
            }

            // 4. Descontar el stock de cada tinta base
            foreach (var tintaDto in resultado.Tintas)
            {
                var tinta = await _unitOfWork.TintaBases.GetByIdAsync(tintaDto.IdTinta);
                tinta.StockActual -= tintaDto.GramosNecesarios;

                // Generar advertencia si queda por debajo del mínimo
                if (tinta.StockActual <= tinta.StockMinimo_alerta)
                    resultado.Advertencias.Add(
                        $"🔴 Stock de '{tinta.NombreTinta}' bajo mínimo: " +
                        $"quedan {tinta.StockActual}g (mínimo: {tinta.StockMinimo_alerta}g).");

                _unitOfWork.TintaBases.Update(tinta);
            }

            await _unitOfWork.SaveAsync();
            resultado.Advertencias.Insert(0, "✅ Mezcla confirmada y stock descontado correctamente.");
            return resultado;
        }
    }
}
