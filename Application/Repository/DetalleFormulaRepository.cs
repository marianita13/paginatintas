using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;
public class DetalleFormulaRepository : GenericRepository<DetalleFormula> , IDetalleFormula
    {
        private readonly paginatintasContext _context;
        public DetalleFormulaRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }