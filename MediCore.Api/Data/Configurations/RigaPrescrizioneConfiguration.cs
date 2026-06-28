using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class RigaPrescrizioneConfiguration : IEntityTypeConfiguration<RigaPrescrizione>
{
    public void Configure(EntityTypeBuilder<RigaPrescrizione> builder)
    {
        builder.HasKey(r => r.RigaPrescrizioneId);

        builder.Property(r => r.Farmaco).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Posologia).IsRequired().HasMaxLength(500);
    }
}
