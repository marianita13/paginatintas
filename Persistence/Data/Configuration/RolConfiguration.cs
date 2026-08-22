
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("rol");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(50);

            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}