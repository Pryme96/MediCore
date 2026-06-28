using MediCore.Api.Dtos.Turni;

namespace MediCore.Api.Services;

public interface ITurnoService
{
    Task<IReadOnlyList<TurnoResponse>> GetAllAsync();
    Task<IReadOnlyList<TurnoResponse>> GetByMedicoAsync(Guid medicoId);
    Task<IReadOnlyList<TurnoResponse>> GetMieiAsync(string userId);
    Task<TurnoResponse?> GetByIdAsync(Guid id);
    Task<(EsitoOperazione Esito, TurnoResponse? Turno)> CreateAsync(TurnoRequest request);
    Task<EsitoOperazione> UpdateAsync(Guid id, TurnoRequest request);
    Task<bool> DeleteAsync(Guid id);
}
