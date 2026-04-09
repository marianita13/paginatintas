using Domain.Entities;
using Domain.Interfaces;
using Persistence;
using Persistence.Data;

namespace Application.Repository
{
    public class RolRepository : GenericRepository<Rol>, IRol
    {
        private readonly paginatintascontext _context;

        public RolRepository(paginatintascontext context) : base(context)
        {
            _context = context;
        }
    }
}