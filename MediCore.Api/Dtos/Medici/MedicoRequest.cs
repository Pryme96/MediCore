using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Medici;

// Dati per creare un medico: crea anche l'account Identity associato.
public record MedicoRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string Nome { get; init; } = null!;

    [Required]
    public string Cognome { get; init; } = null!;

    [Required]
    public string Specializzazione { get; init; } = null!;

    [Required]
    public Guid ServizioId { get; init; }
}
