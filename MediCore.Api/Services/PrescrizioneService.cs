using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Prescrizioni;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class PrescrizioneService(AppDbContext db) : IPrescrizioneService
{
    public async Task<(EsitoOperazione Esito, PrescrizioneResponse? Prescrizione)> CreateAsync(PrescrizioneRequest request, string userId)
    {
        if (request.DataScadenza <= request.DataEmissione)
            return (EsitoOperazione.DatiNonValidi, null);

        // Serve almeno una riga valida (farmaco + posologia + quantità positiva).
        if (request.Righe is null || request.Righe.Count == 0 ||
            request.Righe.Any(r => string.IsNullOrWhiteSpace(r.Farmaco)
                || string.IsNullOrWhiteSpace(r.Posologia)
                || r.Quantita < 1))
            return (EsitoOperazione.DatiNonValidi, null);

        // Il piano terapeutico richiede sempre la diagnosi/indicazione clinica.
        if (request.Tipo == TipoPrescrizione.PianoTerapeutico && string.IsNullOrWhiteSpace(request.Diagnosi))
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
            Tipo = request.Tipo,
            Diagnosi = request.Diagnosi,
            DurataGiorni = request.DurataGiorni,
            Monitoraggio = request.Monitoraggio,
            DataEmissione = request.DataEmissione,
            DataScadenza = request.DataScadenza,
            Note = request.Note,
            NotificaInviata = true,
            OriginAssistita = request.OriginAssistita,
            Righe = request.Righe.Select(r => new RigaPrescrizione
            {
                Farmaco = r.Farmaco,
                Posologia = r.Posologia,
                Quantita = r.Quantita
            }).ToList()
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
            .Include(p => p.Righe)
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
            .Include(p => p.Righe)
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
            .Include(p => p.Righe)
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
        Tipo = prescrizione.Tipo,
        Diagnosi = prescrizione.Diagnosi,
        DurataGiorni = prescrizione.DurataGiorni,
        Monitoraggio = prescrizione.Monitoraggio,
        DataEmissione = prescrizione.DataEmissione,
        DataScadenza = prescrizione.DataScadenza,
        Note = prescrizione.Note,
        NotificaInviata = prescrizione.NotificaInviata,
        OriginAssistita = prescrizione.OriginAssistita,
        Righe = prescrizione.Righe
            .Select(r => new RigaPrescrizioneResponse
            {
                Farmaco = r.Farmaco,
                Posologia = r.Posologia,
                Quantita = r.Quantita
            })
            .ToList()
    };
}
