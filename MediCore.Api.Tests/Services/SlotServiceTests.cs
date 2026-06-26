using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MediCore.Api.Tests.Services;

public class SlotServiceTests
{
    private static async Task<(AppDbContext Db, Turno Turno, Prestazione Prestazione)> SetupAsync(
        TimeOnly oraInizio, TimeOnly oraFine, int durataSlotMin)
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
        var user = new AppUser
        {
            UserName = "medico@medicore.local",
            Email = "medico@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi"
        };
        var medico = new Medico
        {
            UserId = user.Id,
            Specializzazione = "Cardiologia",
            ServizioId = servizio.ServizioId
        };

        // Turno il giorno successivo a oggi: evita slot scartati perché già nel passato
        // e indipendenza del test dal giorno della settimana in cui viene eseguito.
        var domani = DateTime.Now.Date.AddDays(1);
        var giornoSettimana = (GiornoSettimana)(domani.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)domani.DayOfWeek);

        var turno = new Turno
        {
            MedicoId = medico.MedicoId,
            PrestazioneId = prestazione.PrestazioneId,
            GiornoSettimana = giornoSettimana,
            OraInizio = oraInizio,
            OraFine = oraFine,
            DurataSlotMin = durataSlotMin
        };

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.Add(user);
        db.Medici.Add(medico);
        db.Turni.Add(turno);
        await db.SaveChangesAsync();

        return (db, turno, prestazione);
    }

    private static SlotService BuildService(AppDbContext db) =>
        new(db, new ConfigurationBuilder().Build());

    // Numero di occorrenze del giorno settimanale del Turno nella finestra di generazione
    // (default 60 giorni, da oggi incluso), usato per calcolare il numero di slot attesi.
    private static int ContaOccorrenzeNellaFinestra(Turno turno, int giorniFinestra = 60)
    {
        var oggi = DateTime.Now.Date;
        var occorrenze = 0;
        for (var data = oggi; data <= oggi.AddDays(giorniFinestra); data = data.AddDays(1))
        {
            var giornoSettimana = (GiornoSettimana)(data.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)data.DayOfWeek);
            if (giornoSettimana == turno.GiornoSettimana)
                occorrenze++;
        }
        return occorrenze;
    }

    [Fact]
    public async Task GetDisponibiliPerPrestazioneAsync_genera_e_restituisce_gli_slot_liberi()
    {
        var (db, turno, prestazione) = await SetupAsync(new TimeOnly(9, 0), new TimeOnly(12, 0), 60);
        var service = BuildService(db);
        var slotAttesi = ContaOccorrenzeNellaFinestra(turno) * 3; // 9-12 con slot da 60 min = 3 a occorrenza

        var (esito, slot) = await service.GetDisponibiliPerPrestazioneAsync(prestazione.PrestazioneId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(slot);
        Assert.Equal(slotAttesi, slot!.Count);
        Assert.All(slot, s => Assert.Equal(prestazione.PrestazioneId, db.Turni
            .Single(t => t.TurnoId == s.TurnoId).PrestazioneId));
    }

    [Fact]
    public async Task GetDisponibiliPerPrestazioneAsync_con_prestazione_inesistente_restituisce_NonTrovato()
    {
        var db = AppDbContextFactory.Create();
        var service = BuildService(db);

        var (esito, slot) = await service.GetDisponibiliPerPrestazioneAsync(Guid.NewGuid());

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
        Assert.Null(slot);
    }

    [Fact]
    public async Task GetDisponibiliPerPrestazioneAsync_senza_turni_collegati_restituisce_lista_vuota()
    {
        var db = AppDbContextFactory.Create();
        var servizio = new Servizio { Nome = "Cardiologia", Descrizione = "x" };
        var prestazione = new Prestazione
        {
            ServizioId = servizio.ServizioId,
            Nome = "Visita",
            Descrizione = "x",
            DurataMinuti = 30
        };
        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        await db.SaveChangesAsync();

        var service = BuildService(db);
        var (esito, slot) = await service.GetDisponibiliPerPrestazioneAsync(prestazione.PrestazioneId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Empty(slot!);
    }

    [Fact]
    public async Task GetDisponibiliPerPrestazioneAsync_chiamato_due_volte_non_duplica_gli_slot()
    {
        var (db, turno, prestazione) = await SetupAsync(new TimeOnly(9, 0), new TimeOnly(12, 0), 60);
        var service = BuildService(db);
        var slotAttesi = ContaOccorrenzeNellaFinestra(turno) * 3;

        await service.GetDisponibiliPerPrestazioneAsync(prestazione.PrestazioneId);
        await service.GetDisponibiliPerPrestazioneAsync(prestazione.PrestazioneId);

        var totaleSlotGenerati = await db.Slot.CountAsync(s => s.TurnoId == turno.TurnoId);

        Assert.Equal(slotAttesi, totaleSlotGenerati);
    }
}
