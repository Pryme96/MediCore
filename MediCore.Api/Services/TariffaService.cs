using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class TariffaService(AppDbContext db) : ITariffaService
{
    public async Task<IReadOnlyList<TariffaResponse>> GetByPrestazioneAsync(Guid prestazioneId) =>
        await Project(db.Tariffe.AsNoTracking()
            .Where(t => t.PrestazioneId == prestazioneId)
            .OrderBy(t => t.Regime)).ToListAsync();

    public async Task<TariffaResponse?> GetByIdAsync(Guid id) =>
        await Project(db.Tariffe.AsNoTracking().Where(t => t.TariffaId == id))
            .FirstOrDefaultAsync();

    public async Task<(EsitoOperazione Esito, TariffaResponse? Tariffa)> CreateAsync(TariffaRequest request)
    {
        var prestazione = await db.Prestazioni
            .FirstOrDefaultAsync(p => p.PrestazioneId == request.PrestazioneId);
        if (prestazione is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var duplicata = await db.Tariffe
            .AnyAsync(t => t.PrestazioneId == request.PrestazioneId && t.Regime == request.Regime);
        if (duplicata)
            return (EsitoOperazione.Conflitto, null);

        var tariffa = new Tariffa
        {
            PrestazioneId = request.PrestazioneId,
            Regime = request.Regime,
            Prezzo = request.Prezzo
        };

        db.Tariffe.Add(tariffa);
        await db.SaveChangesAsync();

        return (EsitoOperazione.Ok, ToResponse(tariffa, prestazione.Nome));
    }

    public async Task<EsitoOperazione> UpdateAsync(Guid id, TariffaRequest request)
    {
        var tariffa = await db.Tariffe.FirstOrDefaultAsync(t => t.TariffaId == id);
        if (tariffa is null)
            return EsitoOperazione.NonTrovato;

        var prestazioneEsiste = await db.Prestazioni
            .AnyAsync(p => p.PrestazioneId == request.PrestazioneId);
        if (!prestazioneEsiste)
            return EsitoOperazione.RiferimentoNonValido;

        var duplicata = await db.Tariffe.AnyAsync(t =>
            t.TariffaId != id &&
            t.PrestazioneId == request.PrestazioneId &&
            t.Regime == request.Regime);
        if (duplicata)
            return EsitoOperazione.Conflitto;

        tariffa.PrestazioneId = request.PrestazioneId;
        tariffa.Regime = request.Regime;
        tariffa.Prezzo = request.Prezzo;
        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tariffa = await db.Tariffe.FirstOrDefaultAsync(t => t.TariffaId == id);
        if (tariffa is null)
            return false;

        db.Tariffe.Remove(tariffa);
        await db.SaveChangesAsync();
        return true;
    }

    private static IQueryable<TariffaResponse> Project(IQueryable<Tariffa> query) =>
        query.Select(t => new TariffaResponse
        {
            Id = t.TariffaId,
            PrestazioneId = t.PrestazioneId,
            PrestazioneNome = t.Prestazione.Nome,
            Regime = t.Regime,
            Prezzo = t.Prezzo
        });

    private static TariffaResponse ToResponse(Tariffa t, string prestazioneNome) => new()
    {
        Id = t.TariffaId,
        PrestazioneId = t.PrestazioneId,
        PrestazioneNome = prestazioneNome,
        Regime = t.Regime,
        Prezzo = t.Prezzo
    };
}
