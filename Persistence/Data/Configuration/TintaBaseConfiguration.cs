
using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class TintaBaseConfiguration : IEntityTypeConfiguration<TintaBase>
    {
        public void Configure(EntityTypeBuilder<TintaBase> builder)
        {
            builder.ToTable("TintaBase");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.NombreTinta)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(e => e.CodigoHex)
                .HasMaxLength(7);

            builder.Property(e => e.StockActual)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.StockMinimo_alerta)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
        }
    }
}