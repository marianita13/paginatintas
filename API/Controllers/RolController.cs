using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "Administrador")]
public class RolController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RolController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Rol>>> Get()
        {
            var entidades = await _unitOfWork.Roles.GetAllAsync();
            return _mapper.Map<List<Rol>>(entidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDto>> Get(int id)
        {
            var entidad = await _unitOfWork.Roles.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            return _mapper.Map<RolDto>(entidad);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Rol>> Post(RolDto RolDto)
        {
            var entidad = _mapper.Map<Rol>(RolDto);
            this._unitOfWork.Roles.Add(entidad);
            await _unitOfWork.SaveAsync();
            if(entidad == null)
            {
                return BadRequest();
            }
            RolDto.Id = entidad.Id;
            return CreatedAtAction(nameof(Post), new {id = RolDto.Id}, RolDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDto>> Put(int id, [FromBody] RolDto RolDto)
        {
            if(RolDto == null)
            {
                return NotFound();
            }
            var entidades = _mapper.Map<Rol>(RolDto);
            _unitOfWork.Roles.Update(entidades);
            await _unitOfWork.SaveAsync();
            return RolDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.Roles.GetByIdAsync(id);
            if(entidad == null)
            {
                return NotFound();
            }
            _unitOfWork.Roles.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }