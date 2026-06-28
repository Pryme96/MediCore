namespace MediCore.Api.Dtos.Ai;

// Singola riga farmaco proposta dall'assistente, allineata a RigaPrescrizione.
public record RigaSuggerita
{
    public string Farmaco { get; init; } = null!;
    public string Posologia { get; init; } = null!;
    public int Quantita { get; init; }
}
