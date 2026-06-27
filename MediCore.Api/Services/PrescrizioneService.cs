using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Prescrizioni;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class PrescrizioneService(AppDbContext db) : IPrescrizioneService
{
    public async Task<(EsitoOperazione Esito, PrescrizioneResponse? Prescrizione)> CreateAsync(PrescrizioneRequest request, string userId)
    {
        if (request.DataScadenza <= request.DataEmissione)
            return (EsitoOperazione.DatiNonValidi, null);

        var medico = await db.Medici.Include(m => m.User).FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var paziente = await db.Pazienti.Include(p => p.User).FirstOrDefaultAsync(p => p.PazienteId == request.PazienteId);
        if (paziente is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var haPrenotazionePregressa = await db.Prenotazioni
            .AnyAsync(p => p.PazienteId == paziente.PazienteId && p.Slot.Turno.MedicoId == medico.MedicoId);
        if (!haPrenotazionePregressa)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var prescrizione = new Prescrizione
        {
            PazienteId = paziente.PazienteId,
            MedicoId = medico.MedicoId,
            DataEmissione = request.DataEmissione,
            DataScadenza = request.DataScadenza,
            Farmaci = request.Farmaci,
            Note = request.Note,
            NotificaInviata = true
        };
        db.Prescrizioni.Add(prescrizione);
        await db.SaveChangesAsync();

        return (EsitoOperazione.Ok, ToResponse(prescrizione, paziente, medico));
    }

    public async Task<(EsitoOperazione Esito, PrescrizioneResponse? Prescrizione)> GetByIdAsync(Guid id, string userId)
    {
        var prescrizione = await db.Prescrizioni
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Medico).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.PrescrizioneId == id);
        if (prescrizione is null)
            return (EsitoOperazione.NonTrovato, null);

        if (prescrizione.Paziente.UserId != userId && prescrizione.Medico.UserId != userId)
            return (EsitoOperazione.NonAutorizzato, null);

        return (EsitoOperazione.Ok, ToResponse(prescrizione, prescrizione.Paziente, prescrizione.Medico));
    }

    public async Task<IReadOnlyList<PrescrizioneResponse>> GetMieAsync(string userId)
    {
        var paziente = await db.Pazienti.FirstOrDefaultAsync(p => p.UserId == userId);
        if (paziente is null)
            return [];

        var prescrizioni = await db.Prescrizioni.AsNoTracking()
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Medico).ThenInclude(m => m.User)
            .Where(p => p.PazienteId == paziente.PazienteId)
            .OrderByDescending(p => p.DataEmissione)
            .ToListAsync();

        return prescrizioni.Select(p => ToResponse(p, p.Paziente, p.Medico)).ToList();
    }

    public async Task<IReadOnlyList<PrescrizioneResponse>> GetEmesseAsync(string userId)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return [];

        var prescrizioni = await db.Prescrizioni.AsNoTracking()
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Medico).ThenInclude(m => m.User)
            .Where(p => p.MedicoId == medico.MedicoId)
            .OrderByDescending(p => p.DataEmissione)
            .ToListAsync();

        return prescrizioni.Select(p => ToResponse(p, p.Paziente, p.Medico)).ToList();
    }

    private static PrescrizioneResponse ToResponse(Prescrizione prescrizione, Paziente paziente, Medico medico) => new()
    {
        Id = prescrizione.PrescrizioneId,
        PazienteId = paziente.PazienteId,
        PazienteNomeCompleto = $"{paziente.User.Nome} {paziente.User.Cognome}",
        MedicoId = medico.MedicoId,
        MedicoNomeCompleto = $"{medico.User.Nome} {medico.User.Cognome}",
        DataEmissione = prescrizione.DataEmissione,
        DataScadenza = prescrizione.DataScadenza,
        Farmaci = prescrizione.Farmaci,
        Note = prescrizione.Note,
        NotificaInviata = prescrizione.NotificaInviata
    };
}
