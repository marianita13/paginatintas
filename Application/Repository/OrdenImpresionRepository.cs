using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;
using Persistence.Repository;

namespace Persistence.Repository;
public class OrdenImpresionRepository : GenericRepository<OrdenImpresion> , IOrdenImpresionRepository
    {
        private readonly paginatintascontext _context;
        public OrdenImpresionRepository(paginatintascontext context) : base(context)
        {
            _context = context;
        }
    }