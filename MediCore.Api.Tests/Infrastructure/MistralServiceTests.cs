using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Ai;
using MediCore.Api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediCore.Api.Tests.Infrastructure;

public class MistralServiceTests
{
    private static MistralService Crea(string? apiKey)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(apiKey is null ? [] : new Dictionary<string, string?> { ["Mistral:ApiKey"] = apiKey })
            .Build();
        var http = new HttpClient { BaseAddress = new Uri("https://api.mistral.ai") };
        return new MistralService(http, config, NullLogger<MistralService>.Instance);
    }

    [Fact]
    public void ModalitaDemo_e_true_senza_chiave_configurata()
    {
        var service = Crea(apiKey: null);

        Assert.True(service.ModalitaDemo);
    }

    [Fact]
    public async Task SuggerisciAsync_senza_chiave_restituisce_lo_stub_senza_chiamate_di_rete()
    {
        var service = Crea(apiKey: null);
        var dati = new DatiClinici
        {
            Tipo = TipoPrescrizione.Farmacologica,
            Eta = 40,
            Sesso = Sesso.Maschile,
            ContestoClinico = "Cefalea ricorrente"
        };

        var opzioni = await service.SuggerisciAsync(dati);

        Assert.NotEmpty(opzioni);
        Assert.All(opzioni, o => Assert.NotEmpty(o.Righe));
    }

    [Fact]
    public async Task SuggerisciAsync_stub_piano_terapeutico_valorizza_diagnosi()
    {
        var service = Crea(apiKey: null);
        var dati = new DatiClinici
        {
            Tipo = TipoPrescrizione.PianoTerapeutico,
            Eta = 60,
            Sesso = Sesso.Femminile,
            ContestoClinico = "Ipertensione di nuova diagnosi"
        };

        var opzioni = await service.SuggerisciAsync(dati);

        Assert.NotEmpty(opzioni);
        Assert.All(opzioni, o => Assert.False(string.IsNullOrWhiteSpace(o.DiagnosiSuggerita)));
    }
}
