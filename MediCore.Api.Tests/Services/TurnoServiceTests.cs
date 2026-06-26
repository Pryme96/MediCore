using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Turni;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class TurnoServiceTests
{
    private static async Task<(AppDbContext Db, Medico Medico, Prestazione Prestazione)> SetupAsync()
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

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.Add(user);
        db.Medici.Add(medico);
        await db.SaveChangesAsync();

        return (db, medico, prestazione);
    }

    private static TurnoRequest BuildRequest(Medico medico, Prestazione prestazione, TimeOnly inizio, TimeOnly fine) => new()
    {
        MedicoId = medico.MedicoId,
        PrestazioneId = prestazione.PrestazioneId,
        GiornoSettimana = GiornoSettimana.Lunedi,
        OraInizio = inizio,
        OraFine = fine,
        DurataSlotMin = 20
    };

    [Fact]
    public async Task GetAllAsync_restituisce_tutti_i_turni_creati()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        await service.CreateAsync(BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));
        await service.CreateAsync(BuildRequest(medico, prestazione, new TimeOnly(12, 0), new TimeOnly(15, 0)));

        var tutti = await service.GetAllAsync();

        Assert.Equal(2, tutti.Count);
    }

    [Fact]
    public async Task CreateAsync_con_dati_validi_restituisce_Ok()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        var (esito, turno) = await service.CreateAsync(
            BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(turno);
        Assert.Equal("Mario Rossi", turno!.MedicoNomeCompleto);
    }

    [Fact]
    public async Task CreateAsync_con_oraFine_minore_o_uguale_a_oraInizio_restituisce_DatiNonValidi()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        var (esito, turno) = await service.CreateAsync(
            BuildRequest(medico, prestazione, new TimeOnly(12, 0), new TimeOnly(9, 0)));

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(turno);
    }

    [Fact]
    public async Task CreateAsync_con_medico_inesistente_restituisce_RiferimentoNonValido()
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

        var service = new TurnoService(db);
        var (esito, turno) = await service.CreateAsync(new TurnoRequest
        {
            MedicoId = Guid.NewGuid(),
            PrestazioneId = prestazione.PrestazioneId,
            GiornoSettimana = GiornoSettimana.Lunedi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 20
        });

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(turno);
    }

    [Fact]
    public async Task CreateAsync_con_fascia_sovrapposta_per_lo_stesso_medico_restituisce_Conflitto()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        await service.CreateAsync(BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        var (esito, turno) = await service.CreateAsync(
            BuildRequest(medico, prestazione, new TimeOnly(10, 0), new TimeOnly(13, 0)));

        Assert.Equal(EsitoOperazione.Conflitto, esito);
        Assert.Null(turno);
    }

    [Fact]
    public async Task CreateAsync_con_fasce_consecutive_non_sovrapposte_restituisce_Ok()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        await service.CreateAsync(BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        var (esito, turno) = await service.CreateAsync(
            BuildRequest(medico, prestazione, new TimeOnly(12, 0), new TimeOnly(15, 0)));

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(turno);
    }

    [Fact]
    public async Task UpdateAsync_su_turno_inesistente_restituisce_NonTrovato()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        var esito = await service.UpdateAsync(Guid.NewGuid(),
            BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
    }

    [Fact]
    public async Task DeleteAsync_su_turno_esistente_lo_rimuove()
    {
        var (db, medico, prestazione) = await SetupAsync();
        var service = new TurnoService(db);

        var (_, turno) = await service.CreateAsync(
            BuildRequest(medico, prestazione, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        var rimosso = await service.DeleteAsync(turno!.Id);
        var rimossoDiNuovo = await service.DeleteAsync(turno.Id);

        Assert.True(rimosso);
        Assert.False(rimossoDiNuovo);
    }
}
