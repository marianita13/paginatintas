using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Application.Repository
{
    public class FormulaRepository : GenericRepository<Formula>, IFormula
    {
        public FormulaRepository(paginatintasContext context) : base(context) { }

        public async Task<Formula> GetFormulaConDetallesAsync(int idFormula)
        {
            return await _context.Formula
                .Include(f => f.DetalleFormulas)
                    .ThenInclude(d => d.TintaBase)
                .FirstOrDefaultAsync(f => f.Id == idFormula);
        }
    }
}