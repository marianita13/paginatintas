using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;
public class OrdenImpresionRepository : GenericRepository<OrdenImpresion> , IOrdenImpresion
    {
        private readonly paginatintasContext _context;
        public OrdenImpresionRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }