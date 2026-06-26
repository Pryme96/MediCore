namespace MediCore.Api.Dtos.Slot;

// Slot libero restituito al paziente in fase di ricerca.
public record SlotResponse
{
    public Guid Id { get; init; }
    public Guid TurnoId { get; init; }
    public Guid MedicoId { get; init; }
    public string MedicoNomeCompleto { get; init; } = null!;
    public DateTime DataOraInizio { get; init; }
    public DateTime DataOraFine { get; init; }
}
