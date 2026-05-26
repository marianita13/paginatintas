using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Application.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly paginatintasContext _context;

        public GenericRepository(paginatintasContext context)
        {
            _context = context;
        }

        public virtual void Add(T entity) => _context.Set<T>().Add(entity);
        public virtual void AddRange(IEnumerable<T> entities) => _context.Set<T>().AddRange(entities);
        public virtual void Remove(T entity) => _context.Set<T>().Remove(entity);
        public virtual void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);
        public virtual void Update(T entity) => _context.Set<T>().Update(entity);

        public IEnumerable<T> Find(Func<T, bool> predicate)
            => _context.Set<T>().AsEnumerable().Where(predicate);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public virtual async Task<T> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);
    }
}