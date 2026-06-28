namespace MediCore.Api.Dtos.Prescrizioni;

public record RigaPrescrizioneResponse
{
    public string Farmaco { get; init; } = null!;
    public string Posologia { get; init; } = null!;
    public int Quantita { get; init; }
}
