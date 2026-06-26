using MediCore.Api.Dtos.Catalogo;

namespace MediCore.Api.Services;

public interface ITariffaService
{
    Task<IReadOnlyList<TariffaResponse>> GetByPrestazioneAsync(Guid prestazioneId);
    Task<TariffaResponse?> GetByIdAsync(Guid id);
    Task<(EsitoOperazione Esito, TariffaResponse? Tariffa)> CreateAsync(TariffaRequest request);
    Task<EsitoOperazione> UpdateAsync(Guid id, TariffaRequest request);
    Task<bool> DeleteAsync(Guid id);
}
