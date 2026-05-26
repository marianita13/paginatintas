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
public class LogotipocolorController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LogotipocolorController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Logotipocolor>>> Get()
        {
            var entidades = await _unitOfWork.Logotipocolor.GetAllAsync();
            return _mapper.Map<List<Logotipocolor>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipocolorDto>> Get(int id)
        {
            var entidad = await _unitOfWork.Logotipocolor.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<LogotipocolorDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Logotipocolor>> Post(LogotipocolorDto LogotipocolorDto)
        {
            var entidad = _mapper.Map<Logotipocolor>(LogotipocolorDto);
            this._unitOfWork.Logotipocolor.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            LogotipocolorDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = LogotipocolorDto.Id}, LogotipocolorDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipocolorDto>> Put(int id, [FromBody] LogotipocolorDto LogotipocolorDto)
        {
            if(LogotipocolorDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<Logotipocolor>(LogotipocolorDto);
            _unitOfWork.Logotipocolor.Update(entidades);
            await _unitOfWork.SaveAsync();
            return LogotipocolorDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.Logotipocolor.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.Logotipocolor.Delete(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }