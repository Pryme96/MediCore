using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class PrescrizioneConfiguration : IEntityTypeConfiguration<Prescrizione>
{
    public void Configure(EntityTypeBuilder<Prescrizione> builder)
    {
        builder.HasKey(p => p.PrescrizioneId);

        builder.Property(p => p.Diagnosi).HasMaxLength(1000);
        builder.Property(p => p.Monitoraggio).HasMaxLength(1000);
        builder.Property(p => p.Note).HasMaxLength(500);

        builder.HasOne(p => p.Paziente)
            .WithMany(pz => pz.Prescrizioni)
            .HasForeignKey(p => p.PazienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Medico)
            .WithMany(m => m.Prescrizioni)
            .HasForeignKey(p => p.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Le righe seguono il ciclo di vita della prescrizione: eliminandola si eliminano.
        builder.HasMany(p => p.Righe)
            .WithOne(r => r.Prescrizione)
            .HasForeignKey(r => r.PrescrizioneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
