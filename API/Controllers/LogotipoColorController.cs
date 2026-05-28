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
public class LogotipoColorController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LogotipoColorController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<LogotipoColor>>> Get()
        {
            var entidades = await _unitOfWork.LogotipoColors.GetAllAsync();
            return _mapper.Map<List<LogotipoColor>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipoColorDto>> Get(int id)
        {
            var entidad = await _unitOfWork.LogotipoColors.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<LogotipoColorDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LogotipoColor>> Post(LogotipoColorDto LogotipoColorDto)
        {
            var entidad = _mapper.Map<LogotipoColor>(LogotipoColorDto);
            this._unitOfWork.LogotipoColors.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            LogotipoColorDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = LogotipoColorDto.Id}, LogotipoColorDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipoColorDto>> Put(int id, [FromBody] LogotipoColorDto LogotipoColorDto)
        {
            if(LogotipoColorDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<LogotipoColor>(LogotipoColorDto);
            _unitOfWork.LogotipoColors.Update(entidades);
            await _unitOfWork.SaveAsync();
            return LogotipoColorDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.LogotipoColors.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.LogotipoColors.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }