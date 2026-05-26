using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository
{
    public class RolRepository : GenericRepository<Rol>, IRol
    {
        private readonly paginatintasContext _context;

        public RolRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }
}