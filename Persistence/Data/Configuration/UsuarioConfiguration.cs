
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(e => e.Correo)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(e => e.Correo).IsUnique();

            builder.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(e => e.Rol)
                .IsRequired(); // Administrador o Empleado
                
            builder.Property(e => e.PrimerInicio)
                .HasDefaultValue(true);
        }
    }
}