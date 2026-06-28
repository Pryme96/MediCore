using MediCore.Api.Data;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Ai;
using MediCore.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class SuggerimentoService(AppDbContext db, IMistralService mistral) : ISuggerimentoService
{
    public async Task<(EsitoOperazione Esito, SuggerimentoResponse? Risposta)> SuggerisciAsync(SuggerimentoRequest request, string userId)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return (EsitoOperazione.NonAutorizzato, null);

        if (string.IsNullOrWhiteSpace(request.ContestoClinico))
            return (EsitoOperazione.DatiNonValidi, null);

        var paziente = await db.Pazienti.FirstOrDefaultAsync(p => p.PazienteId == request.PazienteId);
        if (paziente is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        // Stessa regola di /prescrizioni: solo per pazienti con cui esiste una prenotazione pregressa.
        var haPrenotazionePregressa = await db.Prenotazioni
            .AnyAsync(p => p.PazienteId == paziente.PazienteId && p.Slot.Turno.MedicoId == medico.MedicoId);
        if (!haPrenotazionePregressa)
            return (EsitoOperazione.RiferimentoNonValido, null);

        // Payload de-identificato: età derivata dalla data di nascita, sesso dal codice fiscale.
        var dati = new DatiClinici
        {
            Tipo = request.Tipo,
            Eta = CalcolaEta(paziente.DataNascita),
            Sesso = DeriveSesso(paziente.CodiceFiscale),
            ContestoClinico = request.ContestoClinico,
            Allergie = string.IsNullOrWhiteSpace(request.Allergie) ? null : request.Allergie,
            TerapieInCorso = string.IsNullOrWhiteSpace(request.TerapieInCorso) ? null : request.TerapieInCorso
        };

        var grezze = await mistral.SuggerisciAsync(dati);

        // L'output dell'assistente non è fidato ciecamente: si tengono solo opzioni con almeno
        // una riga valida, e al massimo tre.
        var opzioni = grezze
            .Where(o => o.Righe.Count > 0
                && o.Righe.All(r => !string.IsNullOrWhiteSpace(r.Farmaco)
                    && !string.IsNullOrWhiteSpace(r.Posologia)
                    && r.Quantita >= 1))
            .Take(3)
            .ToList();

        var risposta = new SuggerimentoResponse
        {
            Opzioni = opzioni,
            DatiInviati = dati,
            Demo = mistral.ModalitaDemo
        };
        return (EsitoOperazione.Ok, risposta);
    }

    private static int CalcolaEta(DateOnly dataNascita)
    {
        var oggi = DateOnly.FromDateTime(DateTime.Today);
        var eta = oggi.Year - dataNascita.Year;
        if (dataNascita > oggi.AddYears(-eta))
            eta--;
        return eta;
    }

    // Nel codice fiscale italiano il giorno di nascita (caratteri 10-11) è aumentato di 40 per
    // le donne. Si ricava il sesso senza inviare il CF all'esterno; null se non parsabile.
    private static Sesso? DeriveSesso(string? codiceFiscale)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscale) || codiceFiscale.Length < 11)
            return null;
        if (!int.TryParse(codiceFiscale.Substring(9, 2), out var giorno))
            return null;
        if (giorno is >= 41 and <= 71)
            return Sesso.Femminile;
        if (giorno is >= 1 and <= 31)
            return Sesso.Maschile;
        return null;
    }
}
