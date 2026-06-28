using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Ai;

// Payload de-identificato effettivamente inviato all'assistente AI (e restituito al frontend
// come DatiInviati, per trasparenza). Nessun identificatore: niente nomi, CF, data di nascita,
// contatti, ID interni. L'età è in anni, il sesso è derivato dal CF (omesso se non parsabile).
public record DatiClinici
{
    public TipoPrescrizione Tipo { get; init; }
    public int Eta { get; init; }
    public Sesso? Sesso { get; init; }
    public string ContestoClinico { get; init; } = null!;
    public string? Allergie { get; init; }
    public string? TerapieInCorso { get; init; }
}
