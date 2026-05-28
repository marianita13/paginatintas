using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class OrdenImpresionController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrdenImpresionController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrdenImpresion>>> Get()
        {
            var entidades = await _unitOfWork.OrdenImpresions.GetAllAsync();
            return _mapper.Map<List<OrdenImpresion>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenImpresionDto>> Get(int id)
        {
            var entidad = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<OrdenImpresionDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrdenImpresion>> Post(OrdenImpresionDto OrdenImpresionDto)
        {
            var entidad = _mapper.Map<OrdenImpresion>(OrdenImpresionDto);
            this._unitOfWork.OrdenImpresions.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            OrdenImpresionDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = OrdenImpresionDto.Id}, OrdenImpresionDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenImpresionDto>> Put(int id, [FromBody] OrdenImpresionDto OrdenImpresionDto)
        {
            if(OrdenImpresionDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<OrdenImpresion>(OrdenImpresionDto);
            _unitOfWork.OrdenImpresions.Update(entidades);
            await _unitOfWork.SaveAsync();
            return OrdenImpresionDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.OrdenImpresions.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.OrdenImpresions.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }