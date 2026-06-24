using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class PrenotazioneConfiguration : IEntityTypeConfiguration<Prenotazione>
{
    public void Configure(EntityTypeBuilder<Prenotazione> builder)
    {
        builder.HasKey(p => p.PrenotazioneId);

        builder.Property(p => p.Note).HasMaxLength(500);

        // Relazione 1..1 con lo slot: genera l'indice unico su SlotId.
        builder.HasOne(p => p.Slot)
            .WithOne(s => s.Prenotazione)
            .HasForeignKey<Prenotazione>(p => p.SlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Paziente)
            .WithMany(pz => pz.Prenotazioni)
            .HasForeignKey(p => p.PazienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
