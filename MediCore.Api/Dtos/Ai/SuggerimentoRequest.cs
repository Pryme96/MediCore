using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Ai;

// Richiesta del frontend per ottenere suggerimenti di redazione.
// PazienteId serve solo lato server (permesso + derivazione età/sesso) e NON viene inoltrato
// all'assistente: i dati che escono sono solo quelli in DatiClinici.
public record SuggerimentoRequest
{
    public Guid PazienteId { get; init; }
    public TipoPrescrizione Tipo { get; init; }
    public string ContestoClinico { get; init; } = null!;
    public string? Allergie { get; init; }
    public string? TerapieInCorso { get; init; }
}
