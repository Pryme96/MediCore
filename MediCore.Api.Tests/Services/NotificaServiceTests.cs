using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.Extensions.Configuration;

namespace MediCore.Api.Tests.Services;

public class NotificaServiceTests
{
    // Config vuota: NotificaService usa la finestra di default di 24 ore.
    private static IConfiguration Config() => new ConfigurationBuilder().Build();

    private static async Task<(AppDbContext Db, Paziente Paziente)> SetupBaseAsync()
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
        var medicoUser = new AppUser { UserName = "medico@medicore.local", Email = "medico@medicore.local", Nome = "Mario", Cognome = "Rossi" };
        var medico = new Medico { UserId = medicoUser.Id, Specializzazione = "Cardiologia", ServizioId = servizio.ServizioId };
        var turno = new Turno
        {
            MedicoId = medico.MedicoId,
            PrestazioneId = prestazione.PrestazioneId,
            GiornoSettimana = GiornoSettimana.Lunedi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 30
        };
        var pazienteUser = new AppUser { UserName = "paziente@medicore.local", Email = "paziente@medicore.local", Nome = "Luca", Cognome = "Bianchi" };
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
        db.Turni.Add(turno);
        db.Pazienti.Add(paziente);
        await db.SaveChangesAsync();

        return (db, paziente);
    }

    private static async Task<Prenotazione> AggiungiPrenotazioneAsync(
        AppDbContext db, Paziente paziente, DateTime dataOraInizio, StatoPrenotazione stato = StatoPrenotazione.Confermata)
    {
        var turno = db.Turni.First();
        var slot = new Slot
        {
            TurnoId = turno.TurnoId,
            DataOraInizio = dataOraInizio,
            DataOraFine = dataOraInizio.AddMinutes(30),
            Stato = StatoSlot.Prenotato
        };
        var prenotazione = new Prenotazione
        {
            PazienteId = paziente.PazienteId,
            SlotId = slot.SlotId,
            Regime = Regime.Privato,
            Stato = stato
        };
        db.Slot.Add(slot);
        db.Prenotazioni.Add(prenotazione);
        await db.SaveChangesAsync();
        return prenotazione;
    }

    [Fact]
    public async Task GeneraPromemoriaDovutiAsync_crea_promemoria_per_appuntamento_imminente()
    {
        var (db, paziente) = await SetupBaseAsync();
        var prenotazione = await AggiungiPrenotazioneAsync(db, paziente, DateTime.Now.AddHours(2));
        var sender = new FakeNotificationSender();
        var service = new NotificaService(db, sender, Config());

        var creati = await service.GeneraPromemoriaDovutiAsync();

        Assert.Equal(1, creati);
        var mie = await service.GetMieAsync(paziente.UserId);
        var promemoria = Assert.Single(mie);
        Assert.Equal(TipoNotifica.PromemoriaAppuntamento, promemoria.Tipo);
        Assert.Equal(prenotazione.PrenotazioneId, promemoria.RiferimentoId);
        Assert.Single(sender.Inviate);
    }

    [Fact]
    public async Task GeneraPromemoriaDovutiAsync_e_idempotente()
    {
        var (db, paziente) = await SetupBaseAsync();
        await AggiungiPrenotazioneAsync(db, paziente, DateTime.Now.AddHours(2));
        var service = new NotificaService(db, new FakeNotificationSender(), Config());

        await service.GeneraPromemoriaDovutiAsync();
        var secondaEsecuzione = await service.GeneraPromemoriaDovutiAsync();

        Assert.Equal(0, secondaEsecuzione);
        Assert.Single(await service.GetMieAsync(paziente.UserId));
    }

    [Fact]
    public async Task GeneraPromemoriaDovutiAsync_ignora_appuntamenti_fuori_finestra()
    {
        var (db, paziente) = await SetupBaseAsync();
        await AggiungiPrenotazioneAsync(db, paziente, DateTime.Now.AddDays(5));
        var service = new NotificaService(db, new FakeNotificationSender(), Config());

        var creati = await service.GeneraPromemoriaDovutiAsync();

        Assert.Equal(0, creati);
    }

    [Fact]
    public async Task GeneraPromemoriaDovutiAsync_ignora_prenotazioni_non_confermate()
    {
        var (db, paziente) = await SetupBaseAsync();
        await AggiungiPrenotazioneAsync(db, paziente, DateTime.Now.AddHours(2), StatoPrenotazione.Annullata);
        var service = new NotificaService(db, new FakeNotificationSender(), Config());

        var creati = await service.GeneraPromemoriaDovutiAsync();

        Assert.Equal(0, creati);
    }

    [Fact]
    public async Task CreateAsync_invia_la_notifica_e_imposta_lo_stato_Inviata()
    {
        var (db, paziente) = await SetupBaseAsync();
        var sender = new FakeNotificationSender();
        var service = new NotificaService(db, sender, Config());

        var notifica = await service.CreateAsync(
            paziente.UserId, TipoNotifica.Prescrizione, "Nuova prescrizione", "Hai una nuova prescrizione.", Guid.CreateVersion7());

        Assert.Equal(StatoInvioNotifica.Inviata, notifica.StatoInvio);
        Assert.NotNull(notifica.DataInvio);
        Assert.Single(sender.Inviate);
    }

    [Fact]
    public async Task GetMieAsync_restituisce_solo_le_notifiche_del_destinatario_non_lette_prima()
    {
        var (db, paziente) = await SetupBaseAsync();
        var service = new NotificaService(db, new FakeNotificationSender(), Config());
        var letta = await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "Vecchia", "...", null);
        await service.MarcaLettaAsync(letta.NotificaId, paziente.UserId);
        await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "Nuova", "...", null);
        await service.CreateAsync("altro-utente", TipoNotifica.Prescrizione, "Di un altro", "...", null);

        var mie = await service.GetMieAsync(paziente.UserId);

        Assert.Equal(2, mie.Count);
        Assert.False(mie[0].Letta); // le non lette compaiono per prime
        Assert.Equal("Nuova", mie[0].Titolo);
    }

    [Fact]
    public async Task ContaNonLetteAsync_conta_solo_le_non_lette_del_destinatario()
    {
        var (db, paziente) = await SetupBaseAsync();
        var service = new NotificaService(db, new FakeNotificationSender(), Config());
        var letta = await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "A", "...", null);
        await service.MarcaLettaAsync(letta.NotificaId, paziente.UserId);
        await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "B", "...", null);

        Assert.Equal(1, await service.ContaNonLetteAsync(paziente.UserId));
    }

    [Fact]
    public async Task MarcaLettaAsync_dal_destinatario_imposta_letta()
    {
        var (db, paziente) = await SetupBaseAsync();
        var service = new NotificaService(db, new FakeNotificationSender(), Config());
        var notifica = await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "A", "...", null);

        var esito = await service.MarcaLettaAsync(notifica.NotificaId, paziente.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(0, await service.ContaNonLetteAsync(paziente.UserId));
    }

    [Fact]
    public async Task MarcaLettaAsync_da_altro_utente_restituisce_NonAutorizzato()
    {
        var (db, paziente) = await SetupBaseAsync();
        var service = new NotificaService(db, new FakeNotificationSender(), Config());
        var notifica = await service.CreateAsync(paziente.UserId, TipoNotifica.Prescrizione, "A", "...", null);

        var esito = await service.MarcaLettaAsync(notifica.NotificaId, "altro-utente");

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
    }

    [Fact]
    public async Task MarcaLettaAsync_inesistente_restituisce_NonTrovato()
    {
        var (db, _) = await SetupBaseAsync();
        var service = new NotificaService(db, new FakeNotificationSender(), Config());

        var esito = await service.MarcaLettaAsync(Guid.NewGuid(), "qualunque");

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
    }
}
