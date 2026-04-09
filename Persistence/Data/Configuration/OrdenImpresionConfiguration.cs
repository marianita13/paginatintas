
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class OrdenImpresionConfiguration : IEntityTypeConfiguration<OrdenImpresion>
    {
        public void Configure(EntityTypeBuilder<OrdenImpresion> builder)
        {
            builder.ToTable("OrdenImpresion");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.VolumenTotal)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.FechaOrden)
                .HasColumnType("timestamp") // Coincide con tu SQL
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Usa el valor por defecto del servidor

            builder.HasOne(o => o.Usuario)
                .WithMany(u => u.OrdenesImpresion)
                .HasForeignKey(o => o.IdUsuario);

            builder.HasOne(o => o.Logotipo)
                .WithMany(l => l.OrdenesImpresion)
                .HasForeignKey(o => o.IdLogotipo);
        }
    }
}