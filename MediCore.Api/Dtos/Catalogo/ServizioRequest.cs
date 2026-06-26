using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Catalogo;

// Dati per creare o aggiornare un servizio.
public record ServizioRequest
{
    [Required, MaxLength(100)]
    public string Nome { get; init; } = null!;

    [Required, MaxLength(500)]
    public string Descrizione { get; init; } = null!;
}
