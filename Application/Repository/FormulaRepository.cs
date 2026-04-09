using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;
using Persistence.Repository;

namespace Persistence.Repository;
public class FormulaRepository : GenericRepository<Formula> , IFormulaRepository
    {
        private readonly paginatintascontext _context;
        public FormulaRepository(paginatintascontext context) : base(context)
        {
            _context = context;
        }
    }