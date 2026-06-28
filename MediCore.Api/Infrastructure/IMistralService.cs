using MediCore.Api.Dtos.Ai;

namespace MediCore.Api.Infrastructure;

// Client dell'assistente di redazione clinica (Mistral, La Plateforme EU).
public interface IMistralService
{
    // true quando manca la chiave API (o è forzata la modalità demo): le opzioni provengono
    // dallo stub dimostrativo e non da una chiamata reale.
    bool ModalitaDemo { get; }

    // Restituisce 0..3 opzioni di redazione per i dati clinici de-identificati forniti.
    // In caso di errore (rete/parsing) restituisce lista vuota: fallback sicuro, mai eccezione.
    Task<IReadOnlyList<SuggerimentoOpzione>> SuggerisciAsync(DatiClinici dati, CancellationToken ct = default);
}
