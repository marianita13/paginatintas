
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class FormulaConfiguration : IEntityTypeConfiguration<Formula>
    {
        public void Configure(EntityTypeBuilder<Formula> builder)
        {
            builder.ToTable("Formula");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.NombreColor)
                .IsRequired()
                .HasMaxLength(50);

            // Relación opcional con Empresa (si es nulo, es una fórmula global)
            builder.HasOne(f => f.Empresa)
                .WithMany(e => e.Formulas)
                .HasForeignKey(f => f.IdEmpresa)
                .IsRequired(false);
        }
    }
}