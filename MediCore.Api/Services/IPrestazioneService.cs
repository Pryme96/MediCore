using MediCore.Api.Dtos.Catalogo;

namespace MediCore.Api.Services;

public interface IPrestazioneService
{
    Task<IReadOnlyList<PrestazioneResponse>> GetAllAsync();
    Task<IReadOnlyList<PrestazioneResponse>> GetByServizioAsync(Guid servizioId);
    Task<PrestazioneResponse?> GetByIdAsync(Guid id);

    // Restituisce null se il servizio indicato non esiste.
    Task<PrestazioneResponse?> CreateAsync(PrestazioneRequest request);

    Task<EsitoOperazione> UpdateAsync(Guid id, PrestazioneRequest request);
}
