using MediCore.Api.Dtos.Pazienti;

namespace MediCore.Api.Services;

public interface IPazienteService
{
    Task<IReadOnlyList<PazienteResponse>> GetAllAsync();
}
