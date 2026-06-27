using MediCore.Api.Dtos.Prescrizioni;

namespace MediCore.Api.Services;

public interface IPrescrizioneService
{
    Task<(EsitoOperazione Esito, PrescrizioneResponse? Prescrizione)> CreateAsync(PrescrizioneRequest request, string userId);
    Task<(EsitoOperazione Esito, PrescrizioneResponse? Prescrizione)> GetByIdAsync(Guid id, string userId);
    Task<IReadOnlyList<PrescrizioneResponse>> GetMieAsync(string userId);
    Task<IReadOnlyList<PrescrizioneResponse>> GetEmesseAsync(string userId);
}
