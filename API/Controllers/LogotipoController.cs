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
public class LogotipoController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LogotipoController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Logotipo>>> Get()
        {
            var entidades = await _unitOfWork.Logotipo.GetAllAsync();
            return _mapper.Map<List<Logotipo>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipoDto>> Get(int id)
        {
            var entidad = await _unitOfWork.Logotipo.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<LogotipoDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Logotipo>> Post(LogotipoDto LogotipoDto)
        {
            var entidad = _mapper.Map<Logotipo>(LogotipoDto);
            this._unitOfWork.Logotipo.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            LogotipoDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = LogotipoDto.Id}, LogotipoDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LogotipoDto>> Put(int id, [FromBody] LogotipoDto LogotipoDto)
        {
            if(LogotipoDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<Logotipo>(LogotipoDto);
            _unitOfWork.Logotipo.Update(entidades);
            await _unitOfWork.SaveAsync();
            return LogotipoDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.Logotipo.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.Logotipo.Delete(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }