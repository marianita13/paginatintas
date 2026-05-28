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
public class EmpresaController: BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmpresaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Empresa>>> Get()
        {
            var Empresaes = await _unitOfWork.Empresas.GetAllAsync();
            return _mapper.Map<List<Empresa>>(Empresaes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpresaDto>> Get(int id)
        {
            var Empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
            if(Empresa == null)
            {
                return NotFound();
            }
            return _mapper.Map<EmpresaDto>(Empresa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Empresa>> Post(EmpresaDto EmpresaDto)
        {
            var Empresa = _mapper.Map<Empresa>(EmpresaDto);
            this._unitOfWork.Empresas.Add(Empresa);
            await _unitOfWork.SaveAsync();
            if(Empresa == null)
            {
                return BadRequest();
            }
            EmpresaDto.Id = Empresa.Id;
            return CreatedAtAction(nameof(Post), new {id = EmpresaDto.Id}, EmpresaDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpresaDto>> Put(int id, [FromBody] EmpresaDto EmpresaDto)
        {
            if(EmpresaDto == null)
            {
                return NotFound();
            }
            var Empresaes = _mapper.Map<Empresa>(EmpresaDto);
            _unitOfWork.Empresas.Update(Empresaes);
            await _unitOfWork.SaveAsync();
            return EmpresaDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var Empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
            if(Empresa == null)
            {
                return NotFound();
            }
            _unitOfWork.Empresas.Remove(Empresa);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }