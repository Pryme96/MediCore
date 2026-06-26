using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class PrestazioneService(AppDbContext db) : IPrestazioneService
{
    public async Task<IReadOnlyList<PrestazioneResponse>> GetAllAsync() =>
        await Project(db.Prestazioni.AsNoTracking().OrderBy(p => p.Nome)).ToListAsync();

    public async Task<IReadOnlyList<PrestazioneResponse>> GetByServizioAsync(Guid servizioId) =>
        await Project(db.Prestazioni.AsNoTracking()
            .Where(p => p.ServizioId == servizioId)
            .OrderBy(p => p.Nome)).ToListAsync();

    public async Task<PrestazioneResponse?> GetByIdAsync(Guid id) =>
        await Project(db.Prestazioni.AsNoTracking().Where(p => p.PrestazioneId == id))
            .FirstOrDefaultAsync();

    public async Task<PrestazioneResponse?> CreateAsync(PrestazioneRequest request)
    {
        var servizio = await db.Servizi.FirstOrDefaultAsync(s => s.ServizioId == request.ServizioId);
        if (servizio is null)
            return null;

        var prestazione = new Prestazione
        {
            ServizioId = request.ServizioId,
            Nome = request.Nome,
            Descrizione = request.Descrizione,
            DurataMinuti = request.DurataMinuti
        };

        db.Prestazioni.Add(prestazione);
        await db.SaveChangesAsync();

        return ToResponse(prestazione, servizio.Nome);
    }

    public async Task<EsitoOperazione> UpdateAsync(Guid id, PrestazioneRequest request)
    {
        var prestazione = await db.Prestazioni.FirstOrDefaultAsync(p => p.PrestazioneId == id);
        if (prestazione is null)
            return EsitoOperazione.NonTrovato;

        var servizioEsiste = await db.Servizi.AnyAsync(s => s.ServizioId == request.ServizioId);
        if (!servizioEsiste)
            return EsitoOperazione.RiferimentoNonValido;

        prestazione.ServizioId = request.ServizioId;
        prestazione.Nome = request.Nome;
        prestazione.Descrizione = request.Descrizione;
        prestazione.DurataMinuti = request.DurataMinuti;
        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    private static IQueryable<PrestazioneResponse> Project(IQueryable<Prestazione> query) =>
        query.Select(p => new PrestazioneResponse
        {
            Id = p.PrestazioneId,
            ServizioId = p.ServizioId,
            ServizioNome = p.Servizio.Nome,
            Nome = p.Nome,
            Descrizione = p.Descrizione,
            DurataMinuti = p.DurataMinuti
        });

    private static PrestazioneResponse ToResponse(Prestazione p, string servizioNome) => new()
    {
        Id = p.PrestazioneId,
        ServizioId = p.ServizioId,
        ServizioNome = servizioNome,
        Nome = p.Nome,
        Descrizione = p.Descrizione,
        DurataMinuti = p.DurataMinuti
    };
}
