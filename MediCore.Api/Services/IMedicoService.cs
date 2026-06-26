using MediCore.Api.Dtos.Medici;

namespace MediCore.Api.Services;

public interface IMedicoService
{
    Task<IReadOnlyList<MedicoResponse>> GetAllAsync();
    Task<MedicoResponse?> GetByIdAsync(Guid id);
    Task<(EsitoOperazione Esito, MedicoCreatoResponse? Medico)> CreateAsync(MedicoRequest request);
    Task<EsitoOperazione> UpdateAsync(Guid id, MedicoUpdateRequest request);
}
