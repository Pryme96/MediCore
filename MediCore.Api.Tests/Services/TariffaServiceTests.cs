using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class TariffaServiceTests
{
    private static async Task<(AppDbContext Db, Prestazione Prestazione)> SetupAsync()
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

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        await db.SaveChangesAsync();

        return (db, prestazione);
    }

    [Fact]
    public async Task CreateAsync_con_prestazione_valida_restituisce_Ok()
    {
        var (db, prestazione) = await SetupAsync();
        var service = new TariffaService(db);

        var (esito, tariffa) = await service.CreateAsync(new TariffaRequest
        {
            PrestazioneId = prestazione.PrestazioneId,
            Regime = Regime.Privato,
            Prezzo = 80
        });

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(tariffa);
        Assert.Equal(80, tariffa!.Prezzo);
    }

    [Fact]
    public async Task CreateAsync_con_prestazione_inesistente_restituisce_RiferimentoNonValido()
    {
        var db = AppDbContextFactory.Create();
        var service = new TariffaService(db);

        var (esito, tariffa) = await service.CreateAsync(new TariffaRequest
        {
            PrestazioneId = Guid.NewGuid(),
            Regime = Regime.Privato,
            Prezzo = 80
        });

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(tariffa);
    }

    [Fact]
    public async Task CreateAsync_con_stessa_prestazione_e_regime_restituisce_Conflitto()
    {
        var (db, prestazione) = await SetupAsync();
        var service = new TariffaService(db);

        await service.CreateAsync(new TariffaRequest
        {
            PrestazioneId = prestazione.PrestazioneId,
            Regime = Regime.Ssn,
            Prezzo = 0
        });

        var (esito, tariffa) = await service.CreateAsync(new TariffaRequest
        {
            PrestazioneId = prestazione.PrestazioneId,
            Regime = Regime.Ssn,
            Prezzo = 10
        });

        Assert.Equal(EsitoOperazione.Conflitto, esito);
        Assert.Null(tariffa);
    }

    [Fact]
    public async Task UpdateAsync_su_tariffa_inesistente_restituisce_NonTrovato()
    {
        var db = AppDbContextFactory.Create();
        var service = new TariffaService(db);

        var esito = await service.UpdateAsync(Guid.NewGuid(), new TariffaRequest
        {
            PrestazioneId = Guid.NewGuid(),
            Regime = Regime.Privato,
            Prezzo = 10
        });

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
    }

    [Fact]
    public async Task DeleteAsync_su_tariffa_esistente_la_rimuove()
    {
        var (db, prestazione) = await SetupAsync();
        var service = new TariffaService(db);

        var (_, tariffa) = await service.CreateAsync(new TariffaRequest
        {
            PrestazioneId = prestazione.PrestazioneId,
            Regime = Regime.Assicurativo,
            Prezzo = 50
        });

        var rimossa = await service.DeleteAsync(tariffa!.Id);
        var rimossaDiNuovo = await service.DeleteAsync(tariffa.Id);

        Assert.True(rimossa);
        Assert.False(rimossaDiNuovo);
    }
}
