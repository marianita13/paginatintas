using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;
[Authorize]
public class FormulaController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FormulaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Formula>>> Get()
        {
            var Formulaes = await _unitOfWork.Formulas.GetAllAsync();
            return _mapper.Map<List<Formula>>(Formulaes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FormulaDto>> Get(int id)
        {
            var Formula = await _unitOfWork.Formulas.GetByIdAsync(id);
            if(Formula == null)
            {
                return NotFound();
            }
            return _mapper.Map<FormulaDto>(Formula);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Formula>> Post(FormulaDto FormulaDto)
        {
            var Formula = _mapper.Map<Formula>(FormulaDto);
            this._unitOfWork.Formulas.Add(Formula);
            await _unitOfWork.SaveAsync();
            if(Formula == null)
            {
                return BadRequest();
            }
            FormulaDto.Id = Formula.Id;
            return CreatedAtAction(nameof(Post), new {id = FormulaDto.Id}, FormulaDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FormulaDto>> Put(int id, [FromBody] FormulaDto FormulaDto)
        {
            if(FormulaDto == null)
            {
                return NotFound();
            }
            var Formulaes = _mapper.Map<Formula>(FormulaDto);
            _unitOfWork.Formulas.Update(Formulaes);
            await _unitOfWork.SaveAsync();
            return FormulaDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var Formula = await _unitOfWork.Formulas.GetByIdAsync(id);
            if(Formula == null)
            {
                return NotFound();
            }
            _unitOfWork.Formulas.Remove(Formula);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }