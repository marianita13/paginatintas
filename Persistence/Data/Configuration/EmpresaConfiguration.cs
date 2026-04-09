
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.ToTable("Empresas");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre_Comercial)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Nit)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(e => e.Nit).IsUnique();

            builder.HasIndex(e => e.Telefono)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(e => e.Correo)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}