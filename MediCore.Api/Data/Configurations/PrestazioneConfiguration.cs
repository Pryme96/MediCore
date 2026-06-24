using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class PrestazioneConfiguration : IEntityTypeConfiguration<Prestazione>
{
    public void Configure(EntityTypeBuilder<Prestazione> builder)
    {
        builder.HasKey(p => p.PrestazioneId);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Descrizione).IsRequired().HasMaxLength(500);

        builder.HasOne(p => p.Servizio)
            .WithMany(s => s.Prestazioni)
            .HasForeignKey(p => p.ServizioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
