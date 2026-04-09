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
public class DetalleFormulaController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DetalleFormulaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<DetalleFormula>>> Get()
        {
            var DetalleFormulaes = await _unitOfWork.DetalleFormula.GetAllAsync();
            return _mapper.Map<List<DetalleFormula>>(DetalleFormulaes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetalleFormulaDto>> Get(int id)
        {
            var DetalleFormula = await _unitOfWork.DetalleFormula.GetByIdAsync(id);
            if(DetalleFormula == null)
            {
                return NotFound();
            }
            return _mapper.Map<DetalleFormulaDto>(DetalleFormula);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DetalleFormula>> Post(DetalleFormulaDto DetalleFormulaDto)
        {
            var DetalleFormula = _mapper.Map<DetalleFormula>(DetalleFormulaDto);
            this._unitOfWork.DetalleFormula.Add(DetalleFormula);
            await _unitOfWork.SaveAsync();
            if(DetalleFormula == null)
            {
                return BadRequest();
            }
            DetalleFormulaDto.Id = DetalleFormula.Id;
            return CreatedAtAction(nameof(Post), new {id = DetalleFormulaDto.Id}, DetalleFormulaDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetalleFormulaDto>> Put(int id, [FromBody] DetalleFormulaDto DetalleFormulaDto)
        {
            if(DetalleFormulaDto == null)
            {
                return NotFound();
            }
            var DetalleFormulaes = _mapper.Map<DetalleFormula>(DetalleFormulaDto);
            _unitOfWork.DetalleFormula.Update(DetalleFormulaes);
            await _unitOfWork.SaveAsync();
            return DetalleFormulaDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var DetalleFormula = await _unitOfWork.DetalleFormula.GetByIdAsync(id);
            if(DetalleFormula == null)
            {
                return NotFound();
            }
            _unitOfWork.DetalleFormula.Delete(DetalleFormula);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }