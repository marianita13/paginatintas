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
            var Empresas = await _unitOfWork.Empresas.GetAllAsync();
            return _mapper.Map<List<Empresa>>(Empresas);
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmpresaDto>> Post([FromBody] EmpresaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
 
        var empresa = new Empresa
        {
            NombreComercial = dto.NombreComercial,
            Telefono        = dto.Telefono ?? string.Empty,
            FechaRegistro   = System.DateTime.UtcNow.ToString("o"),
            Información     = dto.Información,
            Formulas        = null   // evitar que EF intente insertar fórmulas
        };
 
        _unitOfWork.Empresas.Add(empresa);
        await _unitOfWork.SaveAsync();
 
        dto.Id = empresa.Id;
        return CreatedAtAction(nameof(Get), new { id = empresa.Id }, dto);
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
            var empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
            if(empresa == null)
            {
                return NotFound();
            }
            empresa.Información = EmpresaDto.Información;
            empresa.Formulas = null;
            _unitOfWork.Empresas.Update(empresa);
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