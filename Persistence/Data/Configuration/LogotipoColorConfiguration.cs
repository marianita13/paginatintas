using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class LogotipoColorConfiguration : IEntityTypeConfiguration<LogotipoColor>
    {
        public void Configure(EntityTypeBuilder<LogotipoColor> builder)
        {
            builder.ToTable("LogotipoColor");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id);

            builder.HasOne(o => o.Logotipo)
                .WithMany(u => u.LogotipoColors)
                .HasForeignKey(o => o.IdLogotipo);
            
            builder.HasOne(o => o.Formula)
                .WithMany(u => u.LogotipoColors)
                .HasForeignKey(o => o.IdFormula);
        }
    }
}