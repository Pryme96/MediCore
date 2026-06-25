using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Auth;

// Credenziali per il login.
public record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;
}
