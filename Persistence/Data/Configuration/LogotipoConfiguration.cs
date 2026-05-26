
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class LogotipoConfiguration : IEntityTypeConfiguration<Logotipo>
    {
        public void Configure(EntityTypeBuilder<Logotipo> builder)
        {
            builder.ToTable("Logotipo");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.NombreLogo)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(e => e.NombreLogo).IsUnique();

            builder.Property(e => e.UrlImagen)
                .HasMaxLength(255);

            builder.HasOne(l => l.Empresa)
                .WithMany(e => e.Logotipos)
                .HasForeignKey(l => l.IdEmpresa);

            builder.HasOne(l => l.FormulaPrincipal)
                .WithMany()
                .HasForeignKey(l => l.IdFormula);
        }
    }
}