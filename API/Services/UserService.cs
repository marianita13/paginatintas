using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Dtos;
using API.Helpers;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly JWT _jwt;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UserService(IUnitOfWork unitOfWork, IOptions<JWT> jwt, IPasswordHasher<Usuario> passwordHasher)
        {
            _jwt = jwt.Value;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> RegisterAsync(DataUserDto registerDto)
        {
            var usuario = new Usuario
            {
                Nombre = registerDto.Nombre,
                Correo = registerDto.Correo,
                PrimerInicio = 1
            };
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, registerDto.Token); // Token field used as password input

            var existingUser = _unitOfWork.Usuarios
                                    .Find(u => u.Nombre.ToLower() == registerDto.Nombre.ToLower())
                                    .FirstOrDefault();

            if (existingUser == null)
            {
                var rolDefault = _unitOfWork.Roles
                                    .Find(u => u.Nombre == Authorization.rol_default.ToString())
                                    .FirstOrDefault();
                try
                {
                    if (rolDefault != null)
                        usuario.IdRol = rolDefault.Id;

                    _unitOfWork.Usuarios.Add(usuario);
                    await _unitOfWork.SaveAsync();
                    return $"Usuario {registerDto.Nombre} registrado exitosamente.";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
            else
            {
                return $"El usuario {registerDto.Nombre} ya está registrado.";
            }
        }

        public async Task<DataUserDto> GetTokenAsync(LoginDto model)
        {
            var dataUserDto = new DataUserDto();
            var usuario = await _unitOfWork.Usuarios.GetByUsernameAsync(model.Correo);

            if (usuario == null)
            {
                dataUserDto.IsAuthenticated = false;
                dataUserDto.Mensaje = $"No existe un usuario con el Correo {model.Correo}.";
                return dataUserDto;
            }

            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                dataUserDto.IsAuthenticated = true;
                JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
                dataUserDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                dataUserDto.Correo = usuario.Correo;
                dataUserDto.Nombre = usuario.Nombre;
                dataUserDto.Rol = usuario.Rol?.Nombre;

                var activeRefreshToken = usuario.RefreshTokens != null
                    ? System.Linq.Enumerable.FirstOrDefault(usuario.RefreshTokens, a => a.IsActive)
                    : null;

                if (activeRefreshToken != null)
                {
                    dataUserDto.RefreshToken = activeRefreshToken.Token;
                    dataUserDto.RefreshTokenExpiration = activeRefreshToken.Expires;
                }
                else
                {
                    var refreshToken = CreateRefreshToken();
                    dataUserDto.RefreshToken = refreshToken.Token;
                    dataUserDto.RefreshTokenExpiration = refreshToken.Expires;
                    if (usuario.RefreshTokens == null)
                        usuario.RefreshTokens = new List<RefreshToken>();
                    usuario.RefreshTokens.Add(refreshToken);
                    _unitOfWork.Usuarios.Update(usuario);
                    await _unitOfWork.SaveAsync();
                }
                return dataUserDto;
            }

            dataUserDto.IsAuthenticated = false;
            dataUserDto.Mensaje = $"Credenciales incorrectas para el usuario {usuario.Nombre}.";
            return dataUserDto;
        }

        public async Task<DataUserDto> RefreshTokenAsync(string refreshToken)
        {
            var dataUserDto = new DataUserDto();
            var usuario = await _unitOfWork.Usuarios.GetByRefreshTokenAsync(refreshToken);

            if (usuario == null)
            {
                dataUserDto.IsAuthenticated = false;
                dataUserDto.Mensaje = "El token no está asignado a ningún usuario.";
                return dataUserDto;
            }

            var refreshTokenBd = System.Linq.Enumerable.Single(usuario.RefreshTokens, x => x.Token == refreshToken);

            if (!refreshTokenBd.IsActive)
            {
                dataUserDto.IsAuthenticated = false;
                dataUserDto.Mensaje = "El token no está activo.";
                return dataUserDto;
            }

            refreshTokenBd.Revoked = DateTime.UtcNow;
            var newRefreshToken = CreateRefreshToken();
            usuario.RefreshTokens.Add(newRefreshToken);
            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveAsync();

            dataUserDto.IsAuthenticated = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
            dataUserDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            dataUserDto.Correo = usuario.Correo;
            dataUserDto.Nombre = usuario.Nombre;
            dataUserDto.RefreshToken = newRefreshToken.Token;
            dataUserDto.RefreshTokenExpiration = newRefreshToken.Expires;
            return dataUserDto;
        }

        private RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow
            };
        }

        private JwtSecurityToken CreateJwtToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo ?? ""),
                new Claim("uid", usuario.Id.ToString()),
                new Claim("roles", usuario.Rol?.Nombre ?? "")
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);
        }
    }
}
