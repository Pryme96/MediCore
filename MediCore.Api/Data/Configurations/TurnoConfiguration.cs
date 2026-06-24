using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.HasKey(t => t.TurnoId);

        builder.HasOne(t => t.Medico)
            .WithMany(m => m.Turni)
            .HasForeignKey(t => t.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prestazione non espone la collezione inversa: relazione monodirezionale.
        builder.HasOne(t => t.Prestazione)
            .WithMany()
            .HasForeignKey(t => t.PrestazioneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
