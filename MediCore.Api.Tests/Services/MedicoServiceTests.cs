using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Medici;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.AspNetCore.Identity;

namespace MediCore.Api.Tests.Services;

public class MedicoServiceTests
{
    private static async Task<(MediCore.Api.Data.AppDbContext Db, Servizio Servizio)> SetupAsync()
    {
        var db = AppDbContextFactory.Create();
        var servizio = new Servizio { Nome = "Cardiologia", Descrizione = "x" };

        db.Servizi.Add(servizio);
        // Il ruolo deve esistere: UserManager.AddToRoleAsync verifica la presenza in AspNetRoles.
        db.Roles.Add(new IdentityRole(MediCore.Api.Domain.Common.AppRoles.Medico)
        {
            NormalizedName = MediCore.Api.Domain.Common.AppRoles.Medico.ToUpperInvariant()
        });
        await db.SaveChangesAsync();

        return (db, servizio);
    }

    [Fact]
    public async Task CreateAsync_con_dati_validi_crea_account_e_medico()
    {
        var (db, servizio) = await SetupAsync();
        var userManager = UserManagerFactory.Create(db);
        var service = new MedicoService(db, userManager);

        var (esito, risposta) = await service.CreateAsync(new MedicoRequest
        {
            Email = "mario.rossi@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi",
            Specializzazione = "Cardiologia",
            ServizioId = servizio.ServizioId
        });

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(risposta);
        Assert.Equal("mario.rossi@medicore.local", risposta!.Medico.Email);
        Assert.False(string.IsNullOrWhiteSpace(risposta.PasswordGenerata));
        Assert.True(risposta.PasswordGenerata.Length >= 8);
    }

    [Fact]
    public async Task CreateAsync_con_email_gia_registrata_restituisce_Conflitto()
    {
        var (db, servizio) = await SetupAsync();
        var userManager = UserManagerFactory.Create(db);
        var service = new MedicoService(db, userManager);

        var richiesta = new MedicoRequest
        {
            Email = "mario.rossi@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi",
            Specializzazione = "Cardiologia",
            ServizioId = servizio.ServizioId
        };

        await service.CreateAsync(richiesta);
        var (esito, risposta) = await service.CreateAsync(richiesta);

        Assert.Equal(EsitoOperazione.Conflitto, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task CreateAsync_con_servizio_inesistente_restituisce_RiferimentoNonValido()
    {
        var db = AppDbContextFactory.Create();
        var userManager = UserManagerFactory.Create(db);
        var service = new MedicoService(db, userManager);

        var (esito, risposta) = await service.CreateAsync(new MedicoRequest
        {
            Email = "mario.rossi@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi",
            Specializzazione = "Cardiologia",
            ServizioId = Guid.NewGuid()
        });

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task GetAllAsync_restituisce_i_medici_creati()
    {
        var (db, servizio) = await SetupAsync();
        var userManager = UserManagerFactory.Create(db);
        var service = new MedicoService(db, userManager);

        await service.CreateAsync(new MedicoRequest
        {
            Email = "mario.rossi@medicore.local",
            Nome = "Mario",
            Cognome = "Rossi",
            Specializzazione = "Cardiologia",
            ServizioId = servizio.ServizioId
        });

        var tutti = await service.GetAllAsync();

        Assert.Single(tutti);
        Assert.Equal("Rossi", tutti[0].Cognome);
    }

    [Fact]
    public async Task GetByIdAsync_su_medico_inesistente_restituisce_null()
    {
        var db = AppDbContextFactory.Create();
        var userManager = UserManagerFactory.Create(db);
        var service = new MedicoService(db, userManager);

        var risultato = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(risultato);
    }
}
