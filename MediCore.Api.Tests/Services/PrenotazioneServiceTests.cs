using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Prenotazioni;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Tests.Services;

public class PrenotazioneServiceTests
{
    private static async Task<(AppDbContext Db, Paziente Paziente, Slot Slot)> SetupAsync()
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
            DataOraInizio = DateTime.Now.Date.AddDays(7).AddHours(9),
            DataOraFine = DateTime.Now.Date.AddDays(7).AddHours(9).AddMinutes(30),
            Stato = StatoSlot.Libero
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
        db.Turni.Add(turno);
        db.Slot.Add(slot);
        db.Pazienti.Add(paziente);
        await db.SaveChangesAsync();

        return (db, paziente, slot);
    }

    [Fact]
    public async Task CreateAsync_paziente_su_slot_libero_restituisce_Ok_e_occupa_lo_slot()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var request = new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn };

        var (esito, prenotazione) = await service.CreateAsync(request, paziente.UserId, puoPrenotarePerAltri: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prenotazione);
        Assert.Equal(paziente.PazienteId, prenotazione!.PazienteId);
        Assert.Equal(StatoSlot.Prenotato, (await db.Slot.FindAsync(slot.SlotId))!.Stato);
    }

    [Fact]
    public async Task CreateAsync_admin_per_altro_paziente_restituisce_Ok()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var request = new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Privato, PazienteId = paziente.PazienteId };

        var (esito, prenotazione) = await service.CreateAsync(request, "admin-user-id", puoPrenotarePerAltri: true);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(paziente.PazienteId, prenotazione!.PazienteId);
    }

    [Fact]
    public async Task CreateAsync_admin_senza_PazienteId_restituisce_DatiNonValidi()
    {
        var (db, _, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var request = new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn };

        var (esito, prenotazione) = await service.CreateAsync(request, "admin-user-id", puoPrenotarePerAltri: true);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(prenotazione);
    }

    [Fact]
    public async Task CreateAsync_con_slot_inesistente_restituisce_RiferimentoNonValido()
    {
        var (db, paziente, _) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var request = new PrenotazioneRequest { SlotId = Guid.NewGuid(), Regime = Regime.Ssn };

        var (esito, prenotazione) = await service.CreateAsync(request, paziente.UserId, puoPrenotarePerAltri: false);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(prenotazione);
    }

    [Fact]
    public async Task CreateAsync_con_slot_gia_occupato_restituisce_Conflitto()
    {
        var (db, paziente, slot) = await SetupAsync();
        slot.Stato = StatoSlot.Prenotato;
        await db.SaveChangesAsync();
        var service = new PrenotazioneService(db);
        var request = new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn };

        var (esito, prenotazione) = await service.CreateAsync(request, paziente.UserId, puoPrenotarePerAltri: false);

        Assert.Equal(EsitoOperazione.Conflitto, esito);
        Assert.Null(prenotazione);
    }

    [Fact]
    public async Task CreateAsync_su_slot_liberato_dopo_annullamento_restituisce_Ok()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var (_, prima) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);
        await service.AnnullaAsync(prima!.Id, paziente.UserId, isAdmin: false);

        // Lo slot è di nuovo libero: una nuova prenotazione sullo stesso slot deve essere possibile
        // (la riga annullata resta come storico, l'unicità è filtrata sulle non annullate).
        var (esito, seconda) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Privato }, paziente.UserId, puoPrenotarePerAltri: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(seconda);
        Assert.NotEqual(prima.Id, seconda!.Id);
        Assert.Equal(StatoSlot.Prenotato, (await db.Slot.FindAsync(slot.SlotId))!.Stato);
    }

    [Fact]
    public async Task GetByIdAsync_paziente_diverso_dal_proprietario_restituisce_NonAutorizzato()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var (esito, prenotazione) = await service.GetByIdAsync(creata!.Id, "altro-utente", isAdmin: false);

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(prenotazione);
    }

    [Fact]
    public async Task GetByIdAsync_admin_puo_leggere_qualunque_prenotazione()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var (esito, prenotazione) = await service.GetByIdAsync(creata!.Id, "admin-user-id", isAdmin: true);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prenotazione);
    }

    [Fact]
    public async Task GetByIdAsync_medico_titolare_del_turno_puo_leggere()
    {
        var (db, paziente, slot) = await SetupAsync();
        var turno = await db.Turni.SingleAsync(t => t.TurnoId == slot.TurnoId);
        var medico = await db.Medici.SingleAsync(m => m.MedicoId == turno.MedicoId);
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var (esito, prenotazione) = await service.GetByIdAsync(creata!.Id, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(prenotazione);
    }

    [Fact]
    public async Task AnnullaAsync_dal_medico_titolare_libera_lo_slot()
    {
        var (db, paziente, slot) = await SetupAsync();
        var turno = await db.Turni.SingleAsync(t => t.TurnoId == slot.TurnoId);
        var medico = await db.Medici.SingleAsync(m => m.MedicoId == turno.MedicoId);
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var esito = await service.AnnullaAsync(creata!.Id, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(StatoSlot.Libero, (await db.Slot.FindAsync(slot.SlotId))!.Stato);
    }

    [Fact]
    public async Task GetAgendaMedicoAsync_restituisce_le_prenotazioni_sui_propri_turni()
    {
        var (db, paziente, slot) = await SetupAsync();
        var turno = await db.Turni.SingleAsync(t => t.TurnoId == slot.TurnoId);
        var medico = await db.Medici.SingleAsync(m => m.MedicoId == turno.MedicoId);
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var agenda = await service.GetAgendaMedicoAsync(medico.UserId);

        Assert.Single(agenda);
        Assert.Equal(creata!.Id, agenda[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_restituisce_tutte_le_prenotazioni()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var tutte = await service.GetAllAsync();

        Assert.Single(tutte);
        Assert.Equal(paziente.PazienteId, tutte[0].PazienteId);
    }

    [Fact]
    public async Task GetAgendaMedicoAsync_per_utente_non_medico_restituisce_lista_vuota()
    {
        var (db, _, _) = await SetupAsync();
        var service = new PrenotazioneService(db);

        var agenda = await service.GetAgendaMedicoAsync("utente-non-medico");

        Assert.Empty(agenda);
    }

    [Fact]
    public async Task AnnullaAsync_libera_lo_slot_e_imposta_lo_stato_Annullata()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var esito = await service.AnnullaAsync(creata!.Id, paziente.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(StatoSlot.Libero, (await db.Slot.FindAsync(slot.SlotId))!.Stato);
        Assert.Equal(StatoPrenotazione.Annullata, (await db.Prenotazioni.FindAsync(creata.Id))!.Stato);
    }

    [Fact]
    public async Task AnnullaAsync_gia_annullata_restituisce_Conflitto()
    {
        var (db, paziente, slot) = await SetupAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);
        await service.AnnullaAsync(creata!.Id, paziente.UserId, isAdmin: false);

        var esito = await service.AnnullaAsync(creata.Id, paziente.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Conflitto, esito);
    }

    private static async Task<(AppDbContext Db, Medico Medico, Paziente Paziente, Slot Slot)> SetupConTariffaAsync(decimal prezzo = 80)
    {
        var (db, paziente, slot) = await SetupAsync();
        var turno = await db.Turni.SingleAsync(t => t.TurnoId == slot.TurnoId);
        var medico = await db.Medici.SingleAsync(m => m.MedicoId == turno.MedicoId);

        db.Tariffe.Add(new Tariffa { PrestazioneId = turno.PrestazioneId, Regime = Regime.Ssn, Prezzo = prezzo });
        await db.SaveChangesAsync();

        return (db, medico, paziente, slot);
    }

    [Fact]
    public async Task CompletaAsync_dal_medico_titolare_genera_la_Fattura()
    {
        var (db, medico, paziente, slot) = await SetupConTariffaAsync(prezzo: 80);
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var esito = await service.CompletaAsync(creata!.Id, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(StatoPrenotazione.Completata, (await db.Prenotazioni.FindAsync(creata.Id))!.Stato);
        var fattura = await db.Fatture.SingleAsync(f => f.PrenotazioneId == creata.Id);
        Assert.Equal(80, fattura.Importo);
        Assert.Equal(paziente.PazienteId, fattura.PazienteId);
    }

    [Fact]
    public async Task CompletaAsync_da_medico_non_titolare_restituisce_NonAutorizzato()
    {
        var (db, _, paziente, slot) = await SetupConTariffaAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var esito = await service.CompletaAsync(creata!.Id, "medico-estraneo", isAdmin: false);

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
    }

    [Fact]
    public async Task CompletaAsync_senza_tariffa_configurata_restituisce_RiferimentoNonValido()
    {
        var (db, paziente, slot) = await SetupAsync();
        var turno = await db.Turni.SingleAsync(t => t.TurnoId == slot.TurnoId);
        var medico = await db.Medici.SingleAsync(m => m.MedicoId == turno.MedicoId);
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);

        var esito = await service.CompletaAsync(creata!.Id, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
    }

    [Fact]
    public async Task CompletaAsync_gia_completata_restituisce_Conflitto()
    {
        var (db, medico, paziente, slot) = await SetupConTariffaAsync();
        var service = new PrenotazioneService(db);
        var (_, creata) = await service.CreateAsync(
            new PrenotazioneRequest { SlotId = slot.SlotId, Regime = Regime.Ssn }, paziente.UserId, puoPrenotarePerAltri: false);
        await service.CompletaAsync(creata!.Id, medico.UserId, isAdmin: false);

        var esito = await service.CompletaAsync(creata.Id, medico.UserId, isAdmin: false);

        Assert.Equal(EsitoOperazione.Conflitto, esito);
    }
}
