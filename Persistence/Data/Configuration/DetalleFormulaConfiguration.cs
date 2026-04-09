using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class DetalleFormulaConfiguration : IEntityTypeConfiguration<DetalleFormula>
    {
        public void Configure(EntityTypeBuilder<DetalleFormula> builder)
        {
            builder.ToTable("DetalleFormula");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Porcentaje)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Relación: Una TintaBase aparece en muchos detalles
        builder.HasOne(p => p.TintaBase)
            .WithMany(p => p.DetalleFormulas)
            .HasForeignKey(p => p.IdTinta);

        // Relación: Una Formula tiene muchos detalles
        builder.HasOne(p => p.Formula)
            .WithMany(p => p.DetalleFormulas)
            .HasForeignKey(p => p.IdFormula)
            .OnDelete(DeleteBehavior.Cascade); // Si borras la fórmula, se borra el detalle
        }
    }
}