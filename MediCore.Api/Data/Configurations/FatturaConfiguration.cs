using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class FatturaConfiguration : IEntityTypeConfiguration<Fattura>
{
    public void Configure(EntityTypeBuilder<Fattura> builder)
    {
        builder.HasKey(f => f.FatturaId);

        builder.Property(f => f.Importo).HasPrecision(10, 2);

        // Relazione 1..1 con la prenotazione.
        builder.HasOne(f => f.Prenotazione)
            .WithOne(p => p.Fattura)
            .HasForeignKey<Fattura>(f => f.PrenotazioneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Paziente)
            .WithMany(p => p.Fatture)
            .HasForeignKey(f => f.PazienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
