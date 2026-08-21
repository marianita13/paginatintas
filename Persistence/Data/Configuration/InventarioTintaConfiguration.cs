
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class InventarioTintaConfiguration : IEntityTypeConfiguration<InventarioTinta>
    {
        public void Configure(EntityTypeBuilder<InventarioTinta> builder)
        {
            builder.ToTable("InventarioTinta");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.IdInterno)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Lote)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Fabricante)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(e => e.Proveedor)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Presentacion)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.Costo)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.TintaBase)
            .WithMany(p => p.InventarioTintas)
            .HasForeignKey(p => p.IdTintaBase);
        }
    }
}
