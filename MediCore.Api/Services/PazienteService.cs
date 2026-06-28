using MediCore.Api.Data;
using MediCore.Api.Dtos.Pazienti;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class PazienteService(AppDbContext db) : IPazienteService
{
    public async Task<IReadOnlyList<PazienteResponse>> GetAllAsync() =>
        await db.Pazienti.AsNoTracking()
            .OrderBy(p => p.User.Cognome).ThenBy(p => p.User.Nome)
            .Select(p => new PazienteResponse
            {
                Id = p.PazienteId,
                Nome = p.User.Nome,
                Cognome = p.User.Cognome,
                CodiceFiscale = p.CodiceFiscale
            })
            .ToListAsync();
}
