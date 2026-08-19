using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data
{
    public class paginatintasContext : DbContext
    {
        public paginatintasContext(DbContextOptions options) : base(options) { }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Formula> Formula { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<TintaBase> TintaBase { get; set; }
        public DbSet<OrdenImpresion> OrdenImpresion { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<InventarioTinta> InventarioTinta { get; set; }
        public DbSet<DetalleFormula> DetalleFormula { get; set; }
        public DbSet<OrdenFormula> OrdenFormula { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}