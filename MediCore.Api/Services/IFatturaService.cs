using MediCore.Api.Dtos.Fatture;

namespace MediCore.Api.Services;

public interface IFatturaService
{
    Task<(EsitoOperazione Esito, FatturaResponse? Fattura)> GetByIdAsync(Guid id, string userId, bool isAdmin);
    Task<IReadOnlyList<FatturaResponse>> GetMieAsync(string userId);
}
