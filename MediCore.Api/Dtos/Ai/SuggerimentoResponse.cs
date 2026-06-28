namespace MediCore.Api.Dtos.Ai;

// Risposta verso il frontend: 0..3 opzioni (vuoto = fallback sicuro) più l'echo del payload
// de-identificato realmente inviato all'assistente (DatiInviati), per trasparenza.
// Demo = true quando i suggerimenti provengono dalla modalità dimostrativa (nessuna chiave
// configurata), così il frontend può segnalarlo all'utente.
public record SuggerimentoResponse
{
    public IReadOnlyList<SuggerimentoOpzione> Opzioni { get; init; } = [];
    public DatiClinici DatiInviati { get; init; } = null!;
    public bool Demo { get; init; }
}
