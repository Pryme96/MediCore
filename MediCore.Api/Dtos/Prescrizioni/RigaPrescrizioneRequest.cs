namespace MediCore.Api.Dtos.Prescrizioni;

public record RigaPrescrizioneRequest
{
    public string Farmaco { get; init; } = null!;
    public string Posologia { get; init; } = null!;
    public int Quantita { get; init; }
}
