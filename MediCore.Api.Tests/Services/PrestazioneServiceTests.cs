using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class PrestazioneServiceTests
{
    private static async Task<(MediCore.Api.Data.AppDbContext Db, Servizio Servizio)> SetupAsync()
    {
        var db = AppDbContextFactory.Create();
        var servizio = new Servizio { Nome = "Cardiologia", Descrizione = "x" };

        db.Servizi.Add(servizio);
        await db.SaveChangesAsync();

        return (db, servizio);
    }

    [Fact]
    public async Task CreateAsync_con_servizio_valido_restituisce_la_prestazione()
    {
        var (db, servizio) = await SetupAsync();
        var service = new PrestazioneService(db);

        var creata = await service.CreateAsync(new PrestazioneRequest
        {
            ServizioId = servizio.ServizioId,
            Nome = "Visita cardiologica",
            Descrizione = "Prima visita",
            DurataMinuti = 30
        });

        Assert.NotNull(creata);
        Assert.Equal("Cardiologia", creata!.ServizioNome);
    }

    [Fact]
    public async Task CreateAsync_con_servizio_inesistente_restituisce_null()
    {
        var db = AppDbContextFactory.Create();
        var service = new PrestazioneService(db);

        var creata = await service.CreateAsync(new PrestazioneRequest
        {
            ServizioId = Guid.NewGuid(),
            Nome = "Visita cardiologica",
            Descrizione = "x",
            DurataMinuti = 30
        });

        Assert.Null(creata);
    }

    [Fact]
    public async Task GetByServizioAsync_restituisce_solo_le_prestazioni_del_servizio()
    {
        var (db, servizio) = await SetupAsync();
        var altroServizio = new Servizio { Nome = "Ortopedia", Descrizione = "x" };
        db.Servizi.Add(altroServizio);
        await db.SaveChangesAsync();

        var service = new PrestazioneService(db);
        await service.CreateAsync(new PrestazioneRequest
        {
            ServizioId = servizio.ServizioId, Nome = "Visita cardiologica", Descrizione = "x", DurataMinuti = 30
        });
        await service.CreateAsync(new PrestazioneRequest
        {
            ServizioId = altroServizio.ServizioId, Nome = "Visita ortopedica", Descrizione = "x", DurataMinuti = 30
        });

        var risultato = await service.GetByServizioAsync(servizio.ServizioId);

        Assert.Single(risultato);
        Assert.Equal("Visita cardiologica", risultato[0].Nome);
    }

    [Fact]
    public async Task UpdateAsync_su_prestazione_inesistente_restituisce_NonTrovato()
    {
        var db = AppDbContextFactory.Create();
        var service = new PrestazioneService(db);

        var esito = await service.UpdateAsync(Guid.NewGuid(), new PrestazioneRequest
        {
            ServizioId = Guid.NewGuid(), Nome = "x", Descrizione = "y", DurataMinuti = 10
        });

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
    }

    [Fact]
    public async Task UpdateAsync_con_nuovo_servizio_inesistente_restituisce_RiferimentoNonValido()
    {
        var (db, servizio) = await SetupAsync();
        var service = new PrestazioneService(db);
        var creata = await service.CreateAsync(new PrestazioneRequest
        {
            ServizioId = servizio.ServizioId, Nome = "Visita cardiologica", Descrizione = "x", DurataMinuti = 30
        });

        var esito = await service.UpdateAsync(creata!.Id, new PrestazioneRequest
        {
            ServizioId = Guid.NewGuid(), Nome = "x", Descrizione = "y", DurataMinuti = 10
        });

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
    }
}
