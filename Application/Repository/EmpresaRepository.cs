using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;
using Persistence.Repository;

namespace Persistence.Repository;
public class EmpresaRepository : GenericRepository<Empresa> , IEmpresaRepository
    {
        private readonly paginatintascontext _context;
        public EmpresaRepository(paginatintascontext context) : base(context)
        {
            _context = context;
        }
    }