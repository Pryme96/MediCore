using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Prenotazioni;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class PrenotazioneService(AppDbContext db) : IPrenotazioneService
{
    public async Task<(EsitoOperazione Esito, PrenotazioneResponse? Prenotazione)> CreateAsync(PrenotazioneRequest request, string userId, bool puoPrenotarePerAltri)
    {
        Guid pazienteId;
        // Operatore (Amministratore o Medico): prenota per il paziente indicato nel body.
        // Paziente: prenota per sé, il PazienteId è dedotto dall'utente autenticato.
        if (puoPrenotarePerAltri)
        {
            if (request.PazienteId is null)
                return (EsitoOperazione.DatiNonValidi, null);

            var pazienteEsiste = await db.Pazienti.AnyAsync(p => p.PazienteId == request.PazienteId);
            if (!pazienteEsiste)
                return (EsitoOperazione.RiferimentoNonValido, null);

            pazienteId = request.PazienteId.Value;
        }
        else
        {
            var paziente = await db.Pazienti.FirstOrDefaultAsync(p => p.UserId == userId);
            if (paziente is null)
                return (EsitoOperazione.RiferimentoNonValido, null);

            pazienteId = paziente.PazienteId;
        }

        var slot = await db.Slot
            .Include(s => s.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.User)
            .Include(s => s.Turno).ThenInclude(t => t.Prestazione)
            .FirstOrDefaultAsync(s => s.SlotId == request.SlotId);
        if (slot is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        if (slot.Stato != StatoSlot.Libero)
            return (EsitoOperazione.Conflitto, null);

        slot.Stato = StatoSlot.Prenotato;

        var prenotazione = new Prenotazione
        {
            PazienteId = pazienteId,
            SlotId = slot.SlotId,
            Regime = request.Regime,
            Note = request.Note
        };
        db.Prenotazioni.Add(prenotazione);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Copre sia la concorrenza sul token di Slot.Stato sia la violazione dell'indice
            // unico filtrato (un'altra prenotazione attiva è arrivata prima su questo slot).
            return (EsitoOperazione.Conflitto, null);
        }

        var pazienteCreato = await db.Pazienti.Include(p => p.User).FirstAsync(p => p.PazienteId == pazienteId);
        return (EsitoOperazione.Ok, ToResponse(prenotazione, slot, pazienteCreato));
    }

    public async Task<(EsitoOperazione Esito, PrenotazioneResponse? Prenotazione)> GetByIdAsync(Guid id, string userId, bool isAdmin)
    {
        var prenotazione = await db.Prenotazioni
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Prestazione)
            .FirstOrDefaultAsync(p => p.PrenotazioneId == id);
        if (prenotazione is null)
            return (EsitoOperazione.NonTrovato, null);

        if (!PuoAccedere(prenotazione, userId, isAdmin))
            return (EsitoOperazione.NonAutorizzato, null);

        return (EsitoOperazione.Ok, ToResponse(prenotazione, prenotazione.Slot, prenotazione.Paziente));
    }

    public async Task<IReadOnlyList<PrenotazioneResponse>> GetAllAsync()
    {
        var prenotazioni = await db.Prenotazioni.AsNoTracking()
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Prestazione)
            .OrderByDescending(p => p.Slot.DataOraInizio)
            .ToListAsync();

        return prenotazioni.Select(p => ToResponse(p, p.Slot, p.Paziente)).ToList();
    }

    public async Task<IReadOnlyList<PrenotazioneResponse>> GetAgendaMedicoAsync(string userId)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return [];

        var prenotazioni = await db.Prenotazioni.AsNoTracking()
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Prestazione)
            .Where(p => p.Slot.Turno.MedicoId == medico.MedicoId)
            .OrderByDescending(p => p.Slot.DataOraInizio)
            .ToListAsync();

        return prenotazioni.Select(p => ToResponse(p, p.Slot, p.Paziente)).ToList();
    }

    // Accesso consentito all'Amministratore, al Paziente proprietario o al Medico titolare del turno.
    private static bool PuoAccedere(Prenotazione prenotazione, string userId, bool isAdmin) =>
        isAdmin
        || prenotazione.Paziente.UserId == userId
        || prenotazione.Slot.Turno.Medico.UserId == userId;

    public async Task<IReadOnlyList<PrenotazioneResponse>> GetMieAsync(string userId)
    {
        var paziente = await db.Pazienti.FirstOrDefaultAsync(p => p.UserId == userId);
        if (paziente is null)
            return [];

        var prenotazioni = await db.Prenotazioni.AsNoTracking()
            .Include(p => p.Paziente).ThenInclude(pz => pz.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.User)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Prestazione)
            .Where(p => p.PazienteId == paziente.PazienteId)
            .OrderByDescending(p => p.Slot.DataOraInizio)
            .ToListAsync();

        return prenotazioni.Select(p => ToResponse(p, p.Slot, p.Paziente)).ToList();
    }

    public async Task<EsitoOperazione> AnnullaAsync(Guid id, string userId, bool isAdmin)
    {
        var prenotazione = await db.Prenotazioni
            .Include(p => p.Paziente)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico)
            .FirstOrDefaultAsync(p => p.PrenotazioneId == id);
        if (prenotazione is null)
            return EsitoOperazione.NonTrovato;

        if (!PuoAccedere(prenotazione, userId, isAdmin))
            return EsitoOperazione.NonAutorizzato;

        if (prenotazione.Stato != StatoPrenotazione.Confermata)
            return EsitoOperazione.Conflitto;

        prenotazione.Stato = StatoPrenotazione.Annullata;
        prenotazione.Slot.Stato = StatoSlot.Libero;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return EsitoOperazione.Conflitto;
        }

        return EsitoOperazione.Ok;
    }

    // Erogazione della visita: attestazione clinica da parte del medico titolare (o dell'admin).
    // Porta la prenotazione da Confermata a Erogata, senza generare la fattura.
    public async Task<EsitoOperazione> ErogaAsync(Guid id, string userId, bool isAdmin)
    {
        var prenotazione = await db.Prenotazioni
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico)
            .FirstOrDefaultAsync(p => p.PrenotazioneId == id);
        if (prenotazione is null)
            return EsitoOperazione.NonTrovato;

        if (!isAdmin && prenotazione.Slot.Turno.Medico.UserId != userId)
            return EsitoOperazione.NonAutorizzato;

        if (prenotazione.Stato != StatoPrenotazione.Confermata)
            return EsitoOperazione.Conflitto;

        prenotazione.Stato = StatoPrenotazione.Erogata;
        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    // Finalizzazione amministrativa: genera la fattura e porta la prenotazione a Completata.
    // Riservata all'amministratore (vincolo di ruolo applicato dal controller).
    public async Task<EsitoOperazione> CompletaAsync(Guid id)
    {
        var prenotazione = await db.Prenotazioni
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico)
            .FirstOrDefaultAsync(p => p.PrenotazioneId == id);
        if (prenotazione is null)
            return EsitoOperazione.NonTrovato;

        if (prenotazione.Stato != StatoPrenotazione.Erogata)
            return EsitoOperazione.Conflitto;

        var tariffa = await db.Tariffe.FirstOrDefaultAsync(t =>
            t.PrestazioneId == prenotazione.Slot.Turno.PrestazioneId && t.Regime == prenotazione.Regime);
        if (tariffa is null)
            return EsitoOperazione.RiferimentoNonValido;

        prenotazione.Stato = StatoPrenotazione.Completata;

        db.Fatture.Add(new Fattura
        {
            PrenotazioneId = prenotazione.PrenotazioneId,
            PazienteId = prenotazione.PazienteId,
            Importo = tariffa.Prezzo,
            Regime = prenotazione.Regime,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now)
        });

        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    private static PrenotazioneResponse ToResponse(Prenotazione prenotazione, Slot slot, Paziente paziente) => new()
    {
        Id = prenotazione.PrenotazioneId,
        PazienteId = paziente.PazienteId,
        PazienteNomeCompleto = $"{paziente.User.Nome} {paziente.User.Cognome}",
        SlotId = slot.SlotId,
        MedicoNomeCompleto = $"{slot.Turno.Medico.User.Nome} {slot.Turno.Medico.User.Cognome}",
        PrestazioneNome = slot.Turno.Prestazione.Nome,
        DataOraInizio = slot.DataOraInizio,
        DataOraFine = slot.DataOraFine,
        Regime = prenotazione.Regime,
        Stato = prenotazione.Stato,
        Note = prenotazione.Note
    };
}
