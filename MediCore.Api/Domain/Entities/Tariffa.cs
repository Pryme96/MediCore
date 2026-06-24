using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Prezzo di una prestazione per uno specifico regime.
public class Tariffa : AuditableEntity
{
    public Guid TariffaId { get; set; } = Guid.CreateVersion7();
    public Guid PrestazioneId { get; set; }
    public Regime Regime { get; set; }
    public decimal Prezzo { get; set; }

    public Prestazione Prestazione { get; set; } = null!;
}
