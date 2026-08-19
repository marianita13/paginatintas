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
public class OrdenFormulaController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrdenFormulaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrdenFormula>>> Get()
        {
            var entidades = await _unitOfWork.OrdenFormulas.GetAllAsync();
            return _mapper.Map<List<OrdenFormula>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenFormulaDto>> Get(int id)
        {
            var entidad = await _unitOfWork.OrdenFormulas.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<OrdenFormulaDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrdenFormula>> Post(OrdenFormulaDto OrdenFormulaDto)
        {
            var entidad = _mapper.Map<OrdenFormula>(OrdenFormulaDto);
            this._unitOfWork.OrdenFormulas.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            OrdenFormulaDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = OrdenFormulaDto.Id}, OrdenFormulaDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenFormulaDto>> Put(int id, [FromBody] OrdenFormulaDto OrdenFormulaDto)
        {
            if(OrdenFormulaDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<OrdenFormula>(OrdenFormulaDto);
            _unitOfWork.OrdenFormulas.Update(entidades);
            await _unitOfWork.SaveAsync();
            return OrdenFormulaDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.OrdenFormulas.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.OrdenFormulas.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }