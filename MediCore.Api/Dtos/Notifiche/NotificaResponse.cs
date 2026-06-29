using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Notifiche;

public record NotificaResponse
{
    public Guid Id { get; init; }
    public TipoNotifica Tipo { get; init; }
    public string Titolo { get; init; } = null!;
    public string Messaggio { get; init; } = null!;
    public Guid? RiferimentoId { get; init; }
    public bool Letta { get; init; }
    public DateTime DataCreazione { get; init; }
}
