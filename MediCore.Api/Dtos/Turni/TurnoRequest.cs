using System.ComponentModel.DataAnnotations;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Turni;

// Dati per creare o aggiornare un turno settimanale ricorrente.
public record TurnoRequest
{
    [Required]
    public Guid MedicoId { get; init; }

    [Required]
    public Guid PrestazioneId { get; init; }

    [EnumDataType(typeof(GiornoSettimana))]
    public GiornoSettimana GiornoSettimana { get; init; }

    [Required]
    public TimeOnly OraInizio { get; init; }

    [Required]
    public TimeOnly OraFine { get; init; }

    [Range(1, 480)]
    public int DurataSlotMin { get; init; }
}
