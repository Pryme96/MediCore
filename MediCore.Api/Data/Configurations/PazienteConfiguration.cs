using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class PazienteConfiguration : IEntityTypeConfiguration<Paziente>
{
    public void Configure(EntityTypeBuilder<Paziente> builder)
    {
        builder.HasKey(p => p.PazienteId);

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.CodiceFiscale).IsRequired().HasMaxLength(16);
        builder.Property(p => p.Telefono).IsRequired().HasMaxLength(20);

        builder.HasIndex(p => p.CodiceFiscale).IsUnique();

        builder.HasOne(p => p.User)
            .WithOne(u => u.Paziente)
            .HasForeignKey<Paziente>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
