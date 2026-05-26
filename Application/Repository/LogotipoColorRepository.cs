using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Data;

namespace Application.Repository;
public class LogotipoColorRepository : GenericRepository<LogotipoColor> , ILogotipoColor
    {
        private readonly paginatintasContext _context;
        public LogotipoColorRepository(paginatintasContext context) : base(context)
        {
            _context = context;
        }
    }