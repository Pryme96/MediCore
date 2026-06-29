using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Fatture;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class FatturaService(AppDbContext db) : IFatturaService
{
    public async Task<(EsitoOperazione Esito, FatturaResponse? Fattura)> GetByIdAsync(Guid id, string userId, bool isAdmin)
    {
        var fattura = await db.Fatture
            .Include(f => f.Paziente).ThenInclude(p => p.User)
            .Include(f => f.Prenotazione).ThenInclude(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico)
            .FirstOrDefaultAsync(f => f.FatturaId == id);
        if (fattura is null)
            return (EsitoOperazione.NonTrovato, null);

        if (!PuoAccedere(fattura, userId, isAdmin))
            return (EsitoOperazione.NonAutorizzato, null);

        return (EsitoOperazione.Ok, ToResponse(fattura));
    }

    public async Task<IReadOnlyList<FatturaResponse>> GetMieAsync(string userId)
    {
        var paziente = await db.Pazienti.FirstOrDefaultAsync(p => p.UserId == userId);
        if (paziente is null)
            return [];

        var fatture = await db.Fatture.AsNoTracking()
            .Include(f => f.Paziente).ThenInclude(p => p.User)
            .Where(f => f.PazienteId == paziente.PazienteId)
            .OrderByDescending(f => f.DataEmissione)
            .ToListAsync();

        return fatture.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<FatturaResponse>> GetAllAsync()
    {
        var fatture = await db.Fatture.AsNoTracking()
            .Include(f => f.Paziente).ThenInclude(p => p.User)
            .OrderByDescending(f => f.DataEmissione)
            .ToListAsync();

        return fatture.Select(ToResponse).ToList();
    }

    private static bool PuoAccedere(Fattura fattura, string userId, bool isAdmin) =>
        isAdmin || fattura.Paziente.UserId == userId || fattura.Prenotazione.Slot.Turno.Medico.UserId == userId;

    private static FatturaResponse ToResponse(Fattura fattura) => new()
    {
        Id = fattura.FatturaId,
        PrenotazioneId = fattura.PrenotazioneId,
        PazienteId = fattura.PazienteId,
        PazienteNomeCompleto = $"{fattura.Paziente.User.Nome} {fattura.Paziente.User.Cognome}",
        Importo = fattura.Importo,
        Regime = fattura.Regime,
        DataEmissione = fattura.DataEmissione,
        Stato = fattura.Stato
    };
}
