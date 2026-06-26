using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Turni;

// Rappresentazione di un turno restituita dall'API.
public record TurnoResponse
{
    public Guid Id { get; init; }
    public Guid MedicoId { get; init; }
    public string MedicoNomeCompleto { get; init; } = null!;
    public Guid PrestazioneId { get; init; }
    public string PrestazioneNome { get; init; } = null!;
    public GiornoSettimana GiornoSettimana { get; init; }
    public TimeOnly OraInizio { get; init; }
    public TimeOnly OraFine { get; init; }
    public int DurataSlotMin { get; init; }
}
