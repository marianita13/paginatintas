using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class OrdenFormulaConfiguration : IEntityTypeConfiguration<OrdenFormula>
    {
        public void Configure(EntityTypeBuilder<OrdenFormula> builder)
        {
            builder.ToTable("OrdenFormula")
            .HasKey(e => e.Id);

            builder.HasOne(e => e.Formula)
                .WithMany()
                .HasForeignKey(e => e.IdFormula)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.OrdenImpresion)
                .WithMany()
                .HasForeignKey(e => e.IdOrdenImpresion)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}