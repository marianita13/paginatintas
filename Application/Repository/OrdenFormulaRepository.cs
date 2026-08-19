using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;

public class OrdenFormulaRepository : GenericRepository<OrdenFormula> , IOrdenFormula
{
    private readonly paginatintasContext _context;
    public OrdenFormulaRepository(paginatintasContext context) : base(context)
    {
        _context = context;
    }
}
