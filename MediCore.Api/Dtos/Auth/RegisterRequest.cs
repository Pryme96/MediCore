using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Auth;

// Dati per l'auto-registrazione di un paziente (account + profilo).
public record RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = null!;

    [Required, MinLength(8)]
    public string Password { get; init; } = null!;

    [Required]
    public string Nome { get; init; } = null!;

    [Required]
    public string Cognome { get; init; } = null!;

    [Required]
    public string CodiceFiscale { get; init; } = null!;

    [Required]
    public DateOnly DataNascita { get; init; }

    [Required]
    public string Telefono { get; init; } = null!;
}
