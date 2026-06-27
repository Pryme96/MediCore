namespace MediCore.Api.Dtos.Referti;

public record RefertoResponse
{
    public Guid Id { get; init; }
    public Guid PrenotazioneId { get; init; }
    public DateTime DataEmissione { get; init; }
    public string? Contenuto { get; init; }
}
