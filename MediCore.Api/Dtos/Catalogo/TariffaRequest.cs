using System.ComponentModel.DataAnnotations;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Catalogo;

// Dati per creare o aggiornare una tariffa.
public record TariffaRequest
{
    [Required]
    public Guid PrestazioneId { get; init; }

    [EnumDataType(typeof(Regime))]
    public Regime Regime { get; init; }

    [Range(0.01, 1000000)]
    public decimal Prezzo { get; init; }
}
