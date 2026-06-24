using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class ServizioConfiguration : IEntityTypeConfiguration<Servizio>
{
    public void Configure(EntityTypeBuilder<Servizio> builder)
    {
        builder.HasKey(s => s.ServizioId);

        builder.Property(s => s.Nome).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Descrizione).IsRequired().HasMaxLength(500);
    }
}
