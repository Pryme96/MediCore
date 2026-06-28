using MediCore.Api.Dtos.Ai;
using MediCore.Api.Infrastructure;

namespace MediCore.Api.Tests.TestUtils;

// Sostituisce la chiamata reale a Mistral negli unit test: restituisce le opzioni configurate
// e registra l'ultimo payload ricevuto, per verificare cosa verrebbe inviato all'esterno.
public class FakeMistralService : IMistralService
{
    public bool ModalitaDemo { get; set; }
    public IReadOnlyList<SuggerimentoOpzione> Opzioni { get; set; } = [];
    public DatiClinici? UltimiDatiRicevuti { get; private set; }

    public Task<IReadOnlyList<SuggerimentoOpzione>> SuggerisciAsync(DatiClinici dati, CancellationToken ct = default)
    {
        UltimiDatiRicevuti = dati;
        return Task.FromResult(Opzioni);
    }
}
