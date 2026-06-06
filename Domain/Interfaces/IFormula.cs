using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IFormula : IGenericRepository<Formula>
    {
        Task<Formula> GetFormulaConDetallesAsync(int idFormula);
    }
}