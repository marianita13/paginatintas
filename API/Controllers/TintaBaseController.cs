using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.Services;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;
[Authorize]
public class TintaBaseController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TintaBaseController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TintaBase>>> Get()
        {
            var entidades = await _unitOfWork.TintaBases.GetAllAsync();
            return _mapper.Map<List<TintaBase>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TintaBaseDto>> Get(int id)
        {
            var entidad = await _unitOfWork.TintaBases.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<TintaBaseDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TintaBase>> Post(TintaBaseDto TintaBaseDto)
        {
            var entidad = _mapper.Map<TintaBase>(TintaBaseDto);
            this._unitOfWork.TintaBases.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            TintaBaseDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = TintaBaseDto.Id}, TintaBaseDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] TintaBaseDto TintaBaseDto)
        {
            if(TintaBaseDto == null)
            {
                return BadRequest();
            }

            var tintaExistente = await _unitOfWork.TintaBases.GetByIdAsync(id);
            if (tintaExistente == null)
            {
                return NotFound();
            }
            tintaExistente.StockActual = TintaBaseDto.StockActual;
            tintaExistente.StockMinimo_alerta = TintaBaseDto.StockMinimo_alerta;
            tintaExistente.PrecioUnitario = TintaBaseDto.PrecioUnitario;
            _unitOfWork.TintaBases.Update(tintaExistente);
            await _unitOfWork.SaveAsync();
            return Ok(TintaBaseDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.TintaBases.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.TintaBases.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpGet("probar-correo")]
        public async Task<IActionResult> ProbarCorreo([FromServices] IEmailService emailService)
        {
            await emailService.EnviarCorreoStockBajoAsync("Tinta de prueba", 100, 500);
            return Ok("Correo enviado");
        }
    }