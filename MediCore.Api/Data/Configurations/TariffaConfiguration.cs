using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class TariffaConfiguration : IEntityTypeConfiguration<Tariffa>
{
    public void Configure(EntityTypeBuilder<Tariffa> builder)
    {
        builder.HasKey(t => t.TariffaId);

        builder.Property(t => t.Prezzo).HasPrecision(10, 2);

        // Tariffa fissa: una sola riga per coppia prestazione + regime.
        builder.HasIndex(t => new { t.PrestazioneId, t.Regime }).IsUnique();

        builder.HasOne(t => t.Prestazione)
            .WithMany(p => p.Tariffe)
            .HasForeignKey(t => t.PrestazioneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
