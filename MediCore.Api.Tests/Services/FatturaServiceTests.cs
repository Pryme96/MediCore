using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Tests.Services;

public class FatturaServiceTests
{
    private static async Task<(AppDbContext Db, Medico Medico, Paziente Paziente, Fattura Fattura)> SetupAsync()
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
        var slot = new Slot
        {
            TurnoId = turno.TurnoId,
            DataOraInizio = DateTime.Now.Date.AddDays(-7).AddHours(9),
            DataOraFine = DateTime.Now.Date.AddDays(-7).AddHours(9).AddMinutes(30),
            Stato = StatoSlot.Prenotato
        };
        var pazienteUser = new AppUser { UserName = "paziente@medicore.local", Email = "paziente@medicore.local", Nome = "Luca", Cognome = "Bianchi" };
        var paziente = new Paziente
        {
            UserId = pazienteUser.Id,
            CodiceFiscale = "BNCLCU90A01H501A",
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        };
        var prenotazione = new Prenotazione
        {
            PazienteId = paziente.PazienteId,
            SlotId = slot.SlotId,
            Regime = Regime.Ssn,
            Stato = StatoPrenotazione.Completata
        };
        var fattura = new Fattura
        {
            PrenotazioneId = prenotazione.PrenotazioneId,
            PazienteId = paziente.PazienteId,
            Importo = 80,
            Regime = Regime.Ssn,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now)
        };

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.AddRange(medicoUser, pazienteUser);
        db.Medici.Add(medico);
        db.Pazienti.Add(paziente);
        db.Turni.Add(turno);
        db.Slot.Add(slot);
        db.Prenotazioni.Add(prenotazione);
        db.Fatture.Add(fattura);
        await db.SaveChangesAsync();

        return (db, medico, paziente, fattura);
    }

    [Fact]
    public async Task GetByIdAsync_paziente_proprietario_restituisce_Ok()
    {
        var (db, _, paziente, fattura) = await SetupAsync();
        var service = new FatturaService(db);

        var (esito, response) = await service.GetByIdAsync(fattura.FatturaId, paziente.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(response);
        Assert.Equal(80, response!.Importo);
    }

    [Fact]
    public async Task GetByIdAsync_medico_titolare_restituisce_Ok()
    {
        var (db, medico, _, fattura) = await SetupAsync();
        var service = new FatturaService(db);

        var (esito, response) = await service.GetByIdAsync(fattura.FatturaId, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetByIdAsync_amministratore_restituisce_Ok()
    {
        var (db, _, _, fattura) = await SetupAsync();
        var service = new FatturaService(db);

        var (esito, response) = await service.GetByIdAsync(fattura.FatturaId, "admin-user-id", isAdmin: true);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetByIdAsync_utente_estraneo_restituisce_NonAutorizzato()
    {
        var (db, _, _, fattura) = await SetupAsync();
        var service = new FatturaService(db);

        var (esito, response) = await service.GetByIdAsync(fattura.FatturaId, "utente-estraneo", isAdmin: false);

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByIdAsync_inesistente_restituisce_NonTrovato()
    {
        var db = AppDbContextFactory.Create();
        var service = new FatturaService(db);

        var (esito, response) = await service.GetByIdAsync(Guid.NewGuid(), "qualunque", isAdmin: false);

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
        Assert.Null(response);
    }

    [Fact]
    public async Task GetMieAsync_restituisce_solo_le_fatture_del_paziente()
    {
        var (db, _, paziente, _) = await SetupAsync();
        var service = new FatturaService(db);

        var mie = await service.GetMieAsync(paziente.UserId);

        Assert.Single(mie);
        Assert.Equal(paziente.PazienteId, mie[0].PazienteId);
    }

    [Fact]
    public async Task GetAllAsync_restituisce_tutte_le_fatture()
    {
        var (db, _, paziente, fattura) = await SetupAsync();
        var service = new FatturaService(db);

        var tutte = await service.GetAllAsync();

        Assert.Single(tutte);
        Assert.Equal(fattura.FatturaId, tutte[0].Id);
        Assert.Equal(paziente.PazienteId, tutte[0].PazienteId);
    }
}
