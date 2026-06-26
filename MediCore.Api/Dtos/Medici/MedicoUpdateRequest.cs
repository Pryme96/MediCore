using System.ComponentModel.DataAnnotations;

namespace MediCore.Api.Dtos.Medici;

// Dati di dominio modificabili di un medico (account Identity escluso).
public record MedicoUpdateRequest
{
    [Required]
    public string Specializzazione { get; init; } = null!;

    [Required]
    public Guid ServizioId { get; init; }
}
