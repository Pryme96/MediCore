using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.HasKey(m => m.MedicoId);

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.Specializzazione).IsRequired().HasMaxLength(100);

        builder.HasOne(m => m.User)
            .WithOne(u => u.Medico)
            .HasForeignKey<Medico>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Servizio)
            .WithMany(s => s.Medici)
            .HasForeignKey(m => m.ServizioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
