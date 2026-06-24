using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.HasKey(s => s.SlotId);

        // Concorrenza ottimistica: previene il doppio-booking sullo stesso slot.
        builder.Property(s => s.Stato).IsConcurrencyToken();

        // Evita la generazione duplicata dello stesso slot da un turno.
        builder.HasIndex(s => new { s.TurnoId, s.DataOraInizio }).IsUnique();

        builder.HasOne(s => s.Turno)
            .WithMany(t => t.Slot)
            .HasForeignKey(s => s.TurnoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
