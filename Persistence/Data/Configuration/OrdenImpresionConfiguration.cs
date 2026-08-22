
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
            builder.ToTable("ordenimpresion");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.VolumenTotal)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.FechaOrden)
                .HasColumnType("datetime") // Coincide con tu SQL
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Usa el valor por defecto del servidor

            builder.Property(e => e.CostoTotal)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.NumeroCajas)
                .HasColumnType("int");
            
            builder.Property(e => e.PruebaColor)
                .HasColumnType("int");

            builder.Property(e => e.Estado)
                .HasColumnType("tinyint(1)"); // Valor por defecto para el estado

            builder.Property(e => e.NumeroOrden)
                .HasColumnType("varchar(50)")
                .IsRequired(); // Asegura que el número de orden sea obligatorio

            builder.HasOne(o => o.Usuario)
                .WithMany(u => u.OrdenImpresions)
                .HasForeignKey(o => o.IdUsuario);
        }
    }
}