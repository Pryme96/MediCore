using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class PrescrizioneConfiguration : IEntityTypeConfiguration<Prescrizione>
{
    public void Configure(EntityTypeBuilder<Prescrizione> builder)
    {
        builder.HasKey(p => p.PrescrizioneId);

        builder.Property(p => p.Farmaci).IsRequired();
        builder.Property(p => p.Note).HasMaxLength(500);

        builder.HasOne(p => p.Paziente)
            .WithMany(pz => pz.Prescrizioni)
            .HasForeignKey(p => p.PazienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Medico)
            .WithMany(m => m.Prescrizioni)
            .HasForeignKey(p => p.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
