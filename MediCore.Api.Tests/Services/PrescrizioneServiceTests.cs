using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Prescrizioni;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class PrescrizioneServiceTests
{
    private static async Task<(AppDbContext Db, Medico Medico, Paziente Paziente)> SetupAsync(bool conPrenotazionePregressa)
    {
        var db = AppDbContextFactory.Create();

        var servizio = new Servizio { Nome = "Cardiologia", Descrizione = "Servizio di cardiologia" };
        var prestazione = new Prestazione
        {
            ServizioId = servizio.ServizioId,
            Nome = "Visita cardiologica",
            Descrizione = "Prima visita",
            DurataMinuti = 30
        };
        var medicoUser = new AppUser
        {
            UserName = "medico@medicore.local",
            Email = "medico@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi"
        };
        var medico = new Medico
        {
            UserId = medicoUser.Id,
            Specializzazione = "Cardiologia",
            ServizioId = servizio.ServizioId
        };
        var pazienteUser = new AppUser
        {
            UserName = "paziente@medicore.local",
            Email = "paziente@medicore.local",
            Nome = "Luca",
            Cognome = "Bianchi"
        };
        var paziente = new Paziente
        {
            UserId = pazienteUser.Id,
            CodiceFiscale = "BNCLCU90A01H501A",
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        };

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.AddRange(medicoUser, pazienteUser);
        db.Medici.Add(medico);
        db.Pazienti.Add(paziente);

        if (conPrenotazionePregressa)
        {
            var turno = new Turno
            {
                MedicoId = medico.MedicoId,
                PrestazioneId = prestazione.PrestazioneId,
                GiornoSettimana = GiornoSettimana.Lunedi,
                OraInizio = new TimeOnly(9, 0),
                OraFine = new TimeOnly(12, 0),
                DurataSlotMin = 30
            };
            var slot = new Slot
            {
                TurnoId = turno.TurnoId,
                DataOraInizio = DateTime.Now.Date.AddDays(-7).AddHours(9),
                DataOraFine = DateTime.Now.Date.AddDays(-7).AddHours(9).AddMinutes(30),
                Stato = StatoSlot.Prenotato
            };
            var prenotazione = new Prenotazione
            {
                PazienteId = paziente.PazienteId,
                SlotId = slot.SlotId,
                Regime = Regime.Ssn,
                Stato = StatoPrenotazione.Completata
            };
            db.Turni.Add(turno);
            db.Slot.Add(slot);
            db.Prenotazioni.Add(prenotazione);
        }

        await db.SaveChangesAsync();

        return (db, medico, paziente);
    }

    [Fact]
    public async Task CreateAsync_con_prenotazione_pregressa_restituisce_Ok()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prescrizione);
        Assert.True(prescrizione!.NotificaInviata);
    }

    [Fact]
    public async Task CreateAsync_senza_prenotazione_pregressa_restituisce_RiferimentoNonValido()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: false);
        var service = new PrescrizioneService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_con_paziente_inesistente_restituisce_RiferimentoNonValido()
    {
        var (db, medico, _) = await SetupAsync(conPrenotazionePregressa: false);
        var service = new PrescrizioneService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = Guid.NewGuid(),
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_con_DataScadenza_non_successiva_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        var oggi = DateOnly.FromDateTime(DateTime.Now);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = oggi,
            DataScadenza = oggi,
            Farmaci = "Aspirina 100mg"
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task GetByIdAsync_utente_estraneo_restituisce_NonAutorizzato()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        var (_, creata) = await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        }, medico.UserId);

        var (esito, prescrizione) = await service.GetByIdAsync(creata!.Id, "utente-estraneo");

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task GetByIdAsync_paziente_proprietario_restituisce_Ok()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        var (_, creata) = await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        }, medico.UserId);

        var (esito, prescrizione) = await service.GetByIdAsync(creata!.Id, paziente.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prescrizione);
    }

    [Fact]
    public async Task GetMieAsync_restituisce_solo_le_prescrizioni_del_paziente()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        }, medico.UserId);

        var mie = await service.GetMieAsync(paziente.UserId);

        Assert.Single(mie);
        Assert.Equal(paziente.PazienteId, mie[0].PazienteId);
    }

    [Fact]
    public async Task GetEmesseAsync_restituisce_solo_le_prescrizioni_del_medico()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new PrescrizioneService(db);
        await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Aspirina 100mg"
        }, medico.UserId);

        var emesse = await service.GetEmesseAsync(medico.UserId);

        Assert.Single(emesse);
        Assert.Equal(medico.MedicoId, emesse[0].MedicoId);
    }
}
