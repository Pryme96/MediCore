using MediCore.Api.Domain.Entities;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class PazienteServiceTests
{
    [Fact]
    public async Task GetAllAsync_restituisce_i_pazienti_ordinati_per_cognome()
    {
        var db = AppDbContextFactory.Create();
        var userVerdi = new AppUser { UserName = "anna@x", Email = "anna@x", Nome = "Anna", Cognome = "Verdi" };
        var userBianchi = new AppUser { UserName = "luca@x", Email = "luca@x", Nome = "Luca", Cognome = "Bianchi" };
        db.Users.AddRange(userVerdi, userBianchi);
        db.Pazienti.AddRange(
            new Paziente { UserId = userVerdi.Id, CodiceFiscale = "VRDANN90A01H501A", DataNascita = new DateOnly(1990, 1, 1), Telefono = "1" },
            new Paziente { UserId = userBianchi.Id, CodiceFiscale = "BNCLCU91B02H501B", DataNascita = new DateOnly(1991, 2, 2), Telefono = "2" });
        await db.SaveChangesAsync();

        var service = new PazienteService(db);
        var lista = await service.GetAllAsync();

        Assert.Equal(2, lista.Count);
        Assert.Equal("Bianchi", lista[0].Cognome);
        Assert.Equal("Verdi", lista[1].Cognome);
        Assert.Equal("BNCLCU91B02H501B", lista[0].CodiceFiscale);
    }
}
