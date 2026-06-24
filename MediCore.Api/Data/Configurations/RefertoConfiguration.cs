using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class RefertoConfiguration : IEntityTypeConfiguration<Referto>
{
    public void Configure(EntityTypeBuilder<Referto> builder)
    {
        builder.HasKey(r => r.RefertoId);

        builder.Property(r => r.FilePath).HasMaxLength(260);

        // Relazione 1..1 con la prenotazione.
        builder.HasOne(r => r.Prenotazione)
            .WithOne(p => p.Referto)
            .HasForeignKey<Referto>(r => r.PrenotazioneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
