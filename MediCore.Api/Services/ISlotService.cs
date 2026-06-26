using MediCore.Api.Dtos.Slot;

namespace MediCore.Api.Services;

public interface ISlotService
{
    Task<(EsitoOperazione Esito, IReadOnlyList<SlotResponse>? Slot)> GetDisponibiliPerPrestazioneAsync(Guid prestazioneId);
}
