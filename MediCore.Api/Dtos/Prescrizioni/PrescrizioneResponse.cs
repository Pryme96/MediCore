namespace MediCore.Api.Dtos.Prescrizioni;

public record PrescrizioneResponse
{
    public Guid Id { get; init; }
    public Guid PazienteId { get; init; }
    public string PazienteNomeCompleto { get; init; } = null!;
    public Guid MedicoId { get; init; }
    public string MedicoNomeCompleto { get; init; } = null!;
    public DateOnly DataEmissione { get; init; }
    public DateOnly DataScadenza { get; init; }
    public string Farmaci { get; init; } = null!;
    public string? Note { get; init; }
    public bool NotificaInviata { get; init; }
}
