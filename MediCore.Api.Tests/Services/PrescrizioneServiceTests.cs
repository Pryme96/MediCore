using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Prescrizioni;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.Extensions.Configuration;

namespace MediCore.Api.Tests.Services;

public class PrescrizioneServiceTests
{
    // Costruisce il service con un NotificaService reale (sender fittizio): la creazione di una
    // prescrizione genera anche una notifica al paziente.
    private static PrescrizioneService CreaService(AppDbContext db) =>
        new(db, new NotificaService(db, new FakeNotificationSender(), new ConfigurationBuilder().Build()));

    private static IReadOnlyList<RigaPrescrizioneRequest> RigheValide() =>
        [new RigaPrescrizioneRequest { Farmaco = "Aspirina 100mg", Posologia = "1 compressa al giorno", Quantita = 1 }];

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
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prescrizione);
        Assert.True(prescrizione!.NotificaInviata);
        Assert.False(prescrizione.OriginAssistita);
        Assert.Single(prescrizione.Righe);
        Assert.Equal("Aspirina 100mg", prescrizione.Righe[0].Farmaco);
    }

    [Fact]
    public async Task CreateAsync_con_OriginAssistita_true_mantiene_la_provenienza()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            OriginAssistita = true,
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.True(prescrizione!.OriginAssistita);
    }

    [Fact]
    public async Task CreateAsync_piano_terapeutico_con_diagnosi_restituisce_Ok()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.PianoTerapeutico,
            Diagnosi = "Ipertensione arteriosa essenziale",
            DurataGiorni = 180,
            Monitoraggio = "Controllo pressorio mensile",
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prescrizione);
        Assert.Equal(TipoPrescrizione.PianoTerapeutico, prescrizione!.Tipo);
        Assert.Equal("Ipertensione arteriosa essenziale", prescrizione.Diagnosi);
    }

    [Fact]
    public async Task CreateAsync_piano_terapeutico_senza_diagnosi_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.PianoTerapeutico,
            Diagnosi = "   ",
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_senza_righe_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = []
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_con_riga_senza_farmaco_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = [new RigaPrescrizioneRequest { Farmaco = "", Posologia = "1 al giorno", Quantita = 1 }]
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_senza_prenotazione_pregressa_restituisce_RiferimentoNonValido()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: false);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_con_paziente_inesistente_restituisce_RiferimentoNonValido()
    {
        var (db, medico, _) = await SetupAsync(conPrenotazionePregressa: false);
        var service = CreaService(db);
        var request = new PrescrizioneRequest
        {
            PazienteId = Guid.NewGuid(),
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task CreateAsync_con_DataScadenza_non_successiva_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var oggi = DateOnly.FromDateTime(DateTime.Now);
        var request = new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = oggi,
            DataScadenza = oggi,
            Righe = RigheValide()
        };

        var (esito, prescrizione) = await service.CreateAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task GetByIdAsync_utente_estraneo_restituisce_NonAutorizzato()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var (_, creata) = await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        }, medico.UserId);

        var (esito, prescrizione) = await service.GetByIdAsync(creata!.Id, "utente-estraneo");

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(prescrizione);
    }

    [Fact]
    public async Task GetByIdAsync_paziente_proprietario_restituisce_Ok()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        var (_, creata) = await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        }, medico.UserId);

        var (esito, prescrizione) = await service.GetByIdAsync(creata!.Id, paziente.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prescrizione);
        Assert.Single(prescrizione!.Righe);
    }

    [Fact]
    public async Task GetMieAsync_restituisce_solo_le_prescrizioni_del_paziente()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        }, medico.UserId);

        var mie = await service.GetMieAsync(paziente.UserId);

        Assert.Single(mie);
        Assert.Equal(paziente.PazienteId, mie[0].PazienteId);
    }

    [Fact]
    public async Task GetEmesseAsync_restituisce_solo_le_prescrizioni_del_medico()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = CreaService(db);
        await service.CreateAsync(new PrescrizioneRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Righe = RigheValide()
        }, medico.UserId);

        var emesse = await service.GetEmesseAsync(medico.UserId);

        Assert.Single(emesse);
        Assert.Equal(medico.MedicoId, emesse[0].MedicoId);
    }
}
