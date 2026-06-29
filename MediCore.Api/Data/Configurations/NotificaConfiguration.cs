using MediCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCore.Api.Data.Configurations;

public class NotificaConfiguration : IEntityTypeConfiguration<Notifica>
{
    public void Configure(EntityTypeBuilder<Notifica> builder)
    {
        builder.HasKey(n => n.NotificaId);

        builder.Property(n => n.DestinatarioUserId).IsRequired();
        builder.Property(n => n.Titolo).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Messaggio).HasMaxLength(1000).IsRequired();

        builder.HasOne(n => n.Destinatario)
            .WithMany()
            .HasForeignKey(n => n.DestinatarioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Evita notifiche duplicate sulla stessa entità di origine (es. due promemoria sullo
        // stesso appuntamento): unicità su (RiferimentoId, Tipo) quando il riferimento è valorizzato.
        builder.HasIndex(n => new { n.RiferimentoId, n.Tipo })
            .IsUnique()
            .HasFilter("\"RiferimentoId\" IS NOT NULL");

        // Indice di servizio per le query del centro notifiche del paziente.
        builder.HasIndex(n => n.DestinatarioUserId);
    }
}
