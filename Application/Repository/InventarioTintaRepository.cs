using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;
public class InventarioTintaRepository : GenericRepository<InventarioTinta> , IInventarioTinta
    {
        private readonly paginatintasContext _context;
        public InventarioTintaRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }