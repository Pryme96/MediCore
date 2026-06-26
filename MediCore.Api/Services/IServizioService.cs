using MediCore.Api.Dtos.Catalogo;

namespace MediCore.Api.Services;

public interface IServizioService
{
    Task<IReadOnlyList<ServizioResponse>> GetAllAsync();
    Task<ServizioResponse?> GetByIdAsync(Guid id);
    Task<ServizioResponse> CreateAsync(ServizioRequest request);
    Task<bool> UpdateAsync(Guid id, ServizioRequest request);
}
