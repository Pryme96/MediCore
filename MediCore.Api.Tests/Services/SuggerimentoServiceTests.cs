using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Ai;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;

namespace MediCore.Api.Tests.Services;

public class SuggerimentoServiceTests
{
    private static async Task<(AppDbContext Db, Medico Medico, Paziente Paziente)> SetupAsync(
        bool conPrenotazionePregressa, string codiceFiscale = "BNCLCU90A01H501A")
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
            CodiceFiscale = codiceFiscale,
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        };

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.AddRange(medicoUser, pazienteUser);
        db.Medici.Add(medico);
        db.Pazienti.Add(paziente);

        if (conPrenotazionePregressa)
        {
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
            var prenotazione = new Prenotazione
            {
                PazienteId = paziente.PazienteId,
                SlotId = slot.SlotId,
                Regime = Regime.Ssn,
                Stato = StatoPrenotazione.Completata
            };
            db.Turni.Add(turno);
            db.Slot.Add(slot);
            db.Prenotazioni.Add(prenotazione);
        }

        await db.SaveChangesAsync();

        return (db, medico, paziente);
    }

    private static SuggerimentoOpzione OpzioneValida() => new()
    {
        Righe = [new RigaSuggerita { Farmaco = "Paracetamolo 1000mg", Posologia = "1 ogni 8 ore", Quantita = 1 }],
        Motivazione = "Sintomatico di prima linea."
    };

    [Fact]
    public async Task SuggerisciAsync_con_prenotazione_pregressa_restituisce_Ok()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var mistral = new FakeMistralService { Opzioni = [OpzioneValida()] };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (esito, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(risposta);
        Assert.Single(risposta!.Opzioni);
    }

    [Fact]
    public async Task SuggerisciAsync_senza_prenotazione_pregressa_restituisce_RiferimentoNonValido()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: false);
        var service = new SuggerimentoService(db, new FakeMistralService());
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (esito, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task SuggerisciAsync_con_paziente_inesistente_restituisce_RiferimentoNonValido()
    {
        var (db, medico, _) = await SetupAsync(conPrenotazionePregressa: false);
        var service = new SuggerimentoService(db, new FakeMistralService());
        var request = new SuggerimentoRequest
        {
            PazienteId = Guid.NewGuid(),
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (esito, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.RiferimentoNonValido, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task SuggerisciAsync_da_utente_non_medico_restituisce_NonAutorizzato()
    {
        var (db, _, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new SuggerimentoService(db, new FakeMistralService());
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (esito, risposta) = await service.SuggerisciAsync(request, "utente-non-medico");

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task SuggerisciAsync_senza_contesto_clinico_restituisce_DatiNonValidi()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var service = new SuggerimentoService(db, new FakeMistralService());
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "   "
        };

        var (esito, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(risposta);
    }

    [Fact]
    public async Task SuggerisciAsync_deriva_sesso_Femminile_da_codice_fiscale()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true, codiceFiscale: "BNCLCU90A41H501A");
        var mistral = new FakeMistralService { Opzioni = [OpzioneValida()] };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(Sesso.Femminile, mistral.UltimiDatiRicevuti!.Sesso);
    }

    [Fact]
    public async Task SuggerisciAsync_deriva_sesso_Maschile_da_codice_fiscale()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true, codiceFiscale: "BNCLCU90A01H501A");
        var mistral = new FakeMistralService { Opzioni = [OpzioneValida()] };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(Sesso.Maschile, mistral.UltimiDatiRicevuti!.Sesso);
    }

    [Fact]
    public async Task SuggerisciAsync_non_inoltra_dati_identificativi_del_paziente()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var mistral = new FakeMistralService { Opzioni = [OpzioneValida()] };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        await service.SuggerisciAsync(request, medico.UserId);

        // DatiClinici non ha (per tipo) alcun campo identificativo: si verifica che età e
        // contesto clinico siano quelli derivati/forniti, senza riferimenti al paziente.
        // Nascita il 1° gennaio: il compleanno è sempre già trascorso rispetto a "oggi".
        Assert.Equal(DateTime.Today.Year - 1990, mistral.UltimiDatiRicevuti!.Eta);
        Assert.Equal("Cefalea ricorrente", mistral.UltimiDatiRicevuti.ContestoClinico);
    }

    [Fact]
    public async Task SuggerisciAsync_scarta_opzioni_senza_righe_valide()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var mistral = new FakeMistralService
        {
            Opzioni =
            [
                OpzioneValida(),
                new SuggerimentoOpzione { Righe = [], Motivazione = "Senza righe" },
                new SuggerimentoOpzione
                {
                    Righe = [new RigaSuggerita { Farmaco = "", Posologia = "1 al giorno", Quantita = 1 }],
                    Motivazione = "Riga senza farmaco"
                }
            ]
        };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (_, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Single(risposta!.Opzioni);
    }

    [Fact]
    public async Task SuggerisciAsync_limita_a_tre_opzioni()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var mistral = new FakeMistralService
        {
            Opzioni = Enumerable.Range(0, 5).Select(_ => OpzioneValida()).ToList()
        };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (_, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.Equal(3, risposta!.Opzioni.Count);
    }

    [Fact]
    public async Task SuggerisciAsync_riporta_la_modalita_demo_nella_risposta()
    {
        var (db, medico, paziente) = await SetupAsync(conPrenotazionePregressa: true);
        var mistral = new FakeMistralService { Opzioni = [OpzioneValida()], ModalitaDemo = true };
        var service = new SuggerimentoService(db, mistral);
        var request = new SuggerimentoRequest
        {
            PazienteId = paziente.PazienteId,
            Tipo = TipoPrescrizione.Farmacologica,
            ContestoClinico = "Cefalea ricorrente"
        };

        var (_, risposta) = await service.SuggerisciAsync(request, medico.UserId);

        Assert.True(risposta!.Demo);
    }
}
