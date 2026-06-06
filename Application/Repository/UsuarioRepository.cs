using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Repository;
public class UsuarioRepository : GenericRepository<Usuario> , IUsuario
    {
        public UsuarioRepository(paginatintasContext context) : base(context) { }

        public async Task<Usuario> GetByUsernameAsync(string correo)
        {
            return await _context.Usuario
                .Include(u => u.RefreshTokens)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }

        public async Task<Usuario> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Usuario
                .Include(u => u.RefreshTokens)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
        }
    }