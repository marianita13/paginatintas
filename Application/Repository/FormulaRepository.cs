using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;
public class FormulaRepository : GenericRepository<Formula> , IFormula
    {
        private readonly paginatintasContext _context;
        public FormulaRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }