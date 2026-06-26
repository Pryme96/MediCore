using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Catalogo;

// Dati per creare o aggiornare una prestazione.
public record PrestazioneRequest
{
    [Required]
    public Guid ServizioId { get; init; }

    [Required, MaxLength(150)]
    public string Nome { get; init; } = null!;

    [Required, MaxLength(500)]
    public string Descrizione { get; init; } = null!;

    [Range(1, 600)]
    public int DurataMinuti { get; init; }
}
