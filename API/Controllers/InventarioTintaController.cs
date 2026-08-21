using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class InventarioTintaController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventarioTintaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<InventarioTinta>>> Get()
        {
            var InventarioTintaes = await _unitOfWork.InventarioTintas.GetAllAsync();
            return _mapper.Map<List<InventarioTinta>>(InventarioTintaes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventarioTintaDto>> Get(int id)
        {
            var InventarioTinta = await _unitOfWork.InventarioTintas.GetByIdAsync(id);
            if(InventarioTinta == null)
            {
                return NotFound();
            }
            return _mapper.Map<InventarioTintaDto>(InventarioTinta);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventarioTinta>> Post(InventarioTintaDto InventarioTintaDto)
        {
            var InventarioTinta = _mapper.Map<InventarioTinta>(InventarioTintaDto);
            this._unitOfWork.InventarioTintas.Add(InventarioTinta);
            await _unitOfWork.SaveAsync();
            if(InventarioTinta == null)
            {
                return BadRequest();
            }
            InventarioTintaDto.Id = InventarioTinta.Id;
            return CreatedAtAction(nameof(Post), new {id = InventarioTintaDto.Id}, InventarioTintaDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventarioTintaDto>> Put(int id, [FromBody] InventarioTintaDto InventarioTintaDto)
        {
            if(InventarioTintaDto == null)
            {
                return NotFound();
            }
            var InventarioTintaes = _mapper.Map<InventarioTinta>(InventarioTintaDto);
            _unitOfWork.InventarioTintas.Update(InventarioTintaes);
            await _unitOfWork.SaveAsync();
            return InventarioTintaDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var InventarioTinta = await _unitOfWork.InventarioTintas.GetByIdAsync(id);
            if(InventarioTinta == null)
            {
                return NotFound();
            }
            _unitOfWork.InventarioTintas.Remove(InventarioTinta);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }