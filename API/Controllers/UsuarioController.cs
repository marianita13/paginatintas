using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UsuarioController(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        // ── CRUD ────────────────────────────────────────────────

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get()
        {
            var entidades = await _unitOfWork.Usuarios.GetAllAsync();
            return Ok(_mapper.Map<List<UsuarioDto>>(entidades));
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> Get(int id)
        {
            var entidad = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (entidad == null) return NotFound();
            return Ok(_mapper.Map<UsuarioDto>(entidad));
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> Put(int id, [FromBody] UsuarioDto usuarioDto)
        {
            if (usuarioDto == null) return NotFound();
            var entidad = _mapper.Map<Usuario>(usuarioDto);
            _unitOfWork.Usuarios.Update(entidad);
            await _unitOfWork.SaveAsync();
            return Ok(usuarioDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (entidad == null) return NotFound();
            _unitOfWork.Usuarios.Remove(entidad);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        // ── AUTENTICACIÓN ────────────────────────────────────────

        /// <summary>
        /// Registra un nuevo usuario. El rol se asigna automáticamente como Empleado.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistroDto model)
        {
            var resultado = await _userService.RegisterAsync(model);
            return Ok(new { mensaje = resultado });
        }

        /// <summary>
        /// Inicia sesión y devuelve el token JWT.
        /// </summary>
        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetToken([FromBody] LoginDto model)
        {
            var resultado = await _userService.GetTokenAsync(model);
            if (!resultado.IsAuthenticated)
                return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado);
        }

        /// <summary>
        /// Renueva el token JWT usando el refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var resultado = await _userService.RefreshTokenAsync(refreshToken);
            if (!resultado.IsAuthenticated)
                return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado);
        }

        /// <summary>
        /// Asigna un rol a un usuario. Solo Administradores.
        /// </summary>
        [HttpPost("add-rol")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRol([FromBody] AddRolDto model)
        {
            var resultado = await _userService.AddRolAsync(model);
            return Ok(new { mensaje = resultado });
        }
    }
}
