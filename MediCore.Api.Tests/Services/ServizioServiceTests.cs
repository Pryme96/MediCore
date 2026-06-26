using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class ServizioServiceTests
{
    [Fact]
    public async Task CreateAsync_aggiunge_il_servizio_e_lo_restituisce()
    {
        var db = AppDbContextFactory.Create();
        var service = new ServizioService(db);

        var creato = await service.CreateAsync(new ServizioRequest
        {
            Nome = "Cardiologia",
            Descrizione = "Servizio di cardiologia"
        });

        Assert.Equal("Cardiologia", creato.Nome);
        Assert.NotEqual(Guid.Empty, creato.Id);
    }

    [Fact]
    public async Task GetAllAsync_restituisce_i_servizi_ordinati_per_nome()
    {
        var db = AppDbContextFactory.Create();
        var service = new ServizioService(db);

        await service.CreateAsync(new ServizioRequest { Nome = "Ortopedia", Descrizione = "x" });
        await service.CreateAsync(new ServizioRequest { Nome = "Cardiologia", Descrizione = "x" });

        var risultato = await service.GetAllAsync();

        Assert.Equal(["Cardiologia", "Ortopedia"], risultato.Select(s => s.Nome));
    }

    [Fact]
    public async Task GetByIdAsync_su_servizio_inesistente_restituisce_null()
    {
        var db = AppDbContextFactory.Create();
        var service = new ServizioService(db);

        var risultato = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(risultato);
    }

    [Fact]
    public async Task UpdateAsync_su_servizio_esistente_applica_le_modifiche()
    {
        var db = AppDbContextFactory.Create();
        var service = new ServizioService(db);
        var creato = await service.CreateAsync(new ServizioRequest { Nome = "Ortopedia", Descrizione = "x" });

        var aggiornato = await service.UpdateAsync(creato.Id, new ServizioRequest
        {
            Nome = "Ortopedia e Traumatologia",
            Descrizione = "y"
        });
        var risultato = await service.GetByIdAsync(creato.Id);

        Assert.True(aggiornato);
        Assert.Equal("Ortopedia e Traumatologia", risultato!.Nome);
    }

    [Fact]
    public async Task UpdateAsync_su_servizio_inesistente_restituisce_false()
    {
        var db = AppDbContextFactory.Create();
        var service = new ServizioService(db);

        var aggiornato = await service.UpdateAsync(Guid.NewGuid(), new ServizioRequest
        {
            Nome = "x",
            Descrizione = "y"
        });

        Assert.False(aggiornato);
    }
}
