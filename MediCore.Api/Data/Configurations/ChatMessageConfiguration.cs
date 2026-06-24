using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(c => c.MessageId);

        builder.Property(c => c.Contenuto).IsRequired();

        builder.HasOne(c => c.Paziente)
            .WithMany(p => p.ChatMessages)
            .HasForeignKey(c => c.PazienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
