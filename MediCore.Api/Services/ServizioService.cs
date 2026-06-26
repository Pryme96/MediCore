using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class ServizioService(AppDbContext db) : IServizioService
{
    public async Task<IReadOnlyList<ServizioResponse>> GetAllAsync() =>
        await db.Servizi
            .AsNoTracking()
            .OrderBy(s => s.Nome)
            .Select(s => new ServizioResponse
            {
                Id = s.ServizioId,
                Nome = s.Nome,
                Descrizione = s.Descrizione
            })
            .ToListAsync();

    public async Task<ServizioResponse?> GetByIdAsync(Guid id)
    {
        var servizio = await db.Servizi
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServizioId == id);

        return servizio is null ? null : ToResponse(servizio);
    }

    public async Task<ServizioResponse> CreateAsync(ServizioRequest request)
    {
        var servizio = new Servizio
        {
            Nome = request.Nome,
            Descrizione = request.Descrizione
        };

        db.Servizi.Add(servizio);
        await db.SaveChangesAsync();

        return ToResponse(servizio);
    }

    public async Task<bool> UpdateAsync(Guid id, ServizioRequest request)
    {
        var servizio = await db.Servizi.FirstOrDefaultAsync(s => s.ServizioId == id);
        if (servizio is null)
            return false;

        servizio.Nome = request.Nome;
        servizio.Descrizione = request.Descrizione;
        await db.SaveChangesAsync();

        return true;
    }

    private static ServizioResponse ToResponse(Servizio servizio) => new()
    {
        Id = servizio.ServizioId,
        Nome = servizio.Nome,
        Descrizione = servizio.Descrizione
    };
}
