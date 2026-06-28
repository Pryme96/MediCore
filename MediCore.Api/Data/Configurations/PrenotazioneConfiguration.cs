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

        // Relazione con lo slot: uno slot può avere più prenotazioni nel tempo (storico),
        // ma una sola "attiva" alla volta. L'annullamento libera lo slot lasciando la riga
        // storica, quindi l'unicità è filtrata sulle prenotazioni non annullate (Stato <> 2).
        builder.HasOne(p => p.Slot)
            .WithMany()
            .HasForeignKey(p => p.SlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.SlotId)
            .IsUnique()
            .HasFilter("\"Stato\" <> 2");

        builder.HasOne(p => p.Paziente)
            .WithMany(pz => pz.Prenotazioni)
            .HasForeignKey(p => p.PazienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
