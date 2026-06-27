using System.Net;
using System.Net.Http.Json;
using MediCore.Api.IntegrationTests.TestUtils;

namespace MediCore.Api.IntegrationTests;

// Scenario end-to-end contro il server reale (http://localhost:5095).
// Prerequisito: avviare MediCore.Api da Visual Studio (profilo "http") prima di eseguire questi test.
public class EndToEndScenarioTests
{
    private const string PazienteEmail = "mario.rossi@example.com";
    private const string PazientePassword = "Password1!";
    private const string AdminEmail = "admin@medicore.local";
    private const string AdminPassword = "Admin123!";

    [Fact]
    public async Task Scenario_registrazione_e_login_paziente()
    {
        var suffisso = Guid.NewGuid().ToString("N")[..8];
        var client = new ApiClient();
        var email = $"paziente.{suffisso}@example.com";

        // 1. Auto-registrazione di un nuovo paziente.
        var registerResponse = await client.PostAsync("auth/register", new
        {
            Email = email,
            Password = "Password1!",
            Nome = "Test",
            Cognome = $"Paziente{suffisso}",
            CodiceFiscale = $"TST{suffisso.ToUpperInvariant()}",
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // 2. La stessa email non si può registrare due volte.
        var registerDuplicatoResponse = await client.PostAsync("auth/register", new
        {
            Email = email,
            Password = "Password1!",
            Nome = "Test",
            Cognome = "Paziente",
            CodiceFiscale = $"TST{suffisso.ToUpperInvariant()}2",
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        });
        Assert.Equal(HttpStatusCode.Conflict, registerDuplicatoResponse.StatusCode);

        // 3. Login con l'account appena creato.
        var token = await client.LoginAsync(email, "Password1!");
        Assert.False(string.IsNullOrWhiteSpace(token));

        // 4. /auth/me restituisce i dati dell'utente autenticato, incluso il ruolo Paziente.
        client.UseToken(token);
        var meResponse = await client.GetAsync("auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<UserInfoResponse>();
        Assert.Equal(email, me!.Email);
        Assert.Contains("Paziente", me.Ruoli);

        // 5. Senza token, /auth/me è inaccessibile.
        client.UseToken(null);
        var meSenzaTokenResponse = await client.GetAsync("auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meSenzaTokenResponse.StatusCode);
    }

    [Fact]
    public async Task Scenario_completo_admin_crea_catalogo_medico_e_turno()
    {
        var suffisso = Guid.NewGuid().ToString("N")[..8];
        var client = new ApiClient();

        // 1. Login paziente esistente.
        var tokenPaziente = await client.LoginAsync(PazienteEmail, PazientePassword);
        Assert.False(string.IsNullOrWhiteSpace(tokenPaziente));

        // 2. Il paziente legge il catalogo servizi (endpoint autenticato, nessun ruolo specifico).
        client.UseToken(tokenPaziente);
        var getServiziComePaziente = await client.GetAsync("servizi");
        Assert.Equal(HttpStatusCode.OK, getServiziComePaziente.StatusCode);

        // 3. Il paziente NON può creare un servizio (solo Amministratore).
        var creaServizioComePaziente = await client.PostAsync("servizi", new { Nome = "Test", Descrizione = "Test" });
        Assert.Equal(HttpStatusCode.Forbidden, creaServizioComePaziente.StatusCode);

        // 4. Login amministratore.
        var tokenAdmin = await client.LoginAsync(AdminEmail, AdminPassword);
        client.UseToken(tokenAdmin);

        // 5. Crea Servizio.
        var servizioResponse = await client.PostAsync("servizi", new
        {
            Nome = $"Servizio integrazione {suffisso}",
            Descrizione = "Creato dal test di integrazione"
        });
        Assert.Equal(HttpStatusCode.Created, servizioResponse.StatusCode);
        var servizio = await servizioResponse.Content.ReadFromJsonAsync<ServizioResponse>();
        Assert.NotNull(servizio);

        // 6. Crea Prestazione collegata al Servizio.
        var prestazioneResponse = await client.PostAsync("prestazioni", new
        {
            ServizioId = servizio!.Id,
            Nome = $"Prestazione integrazione {suffisso}",
            Descrizione = "Creata dal test di integrazione",
            DurataMinuti = 30
        });
        Assert.Equal(HttpStatusCode.Created, prestazioneResponse.StatusCode);
        var prestazione = await prestazioneResponse.Content.ReadFromJsonAsync<PrestazioneResponse>();
        Assert.NotNull(prestazione);

        // 6bis. Prestazione su Servizio inesistente -> 400.
        var prestazioneServizioInesistenteResponse = await client.PostAsync("prestazioni", new
        {
            ServizioId = Guid.NewGuid(),
            Nome = "x",
            Descrizione = "x",
            DurataMinuti = 30
        });
        Assert.Equal(HttpStatusCode.BadRequest, prestazioneServizioInesistenteResponse.StatusCode);

        // 6ter. Crea una Tariffa per la Prestazione e verifica l'unicità Prestazione+Regime.
        var tariffaResponse = await client.PostAsync("tariffe", new
        {
            PrestazioneId = prestazione!.Id,
            Regime = Regime.Privato,
            Prezzo = 80
        });
        Assert.Equal(HttpStatusCode.Created, tariffaResponse.StatusCode);
        var tariffa = await tariffaResponse.Content.ReadFromJsonAsync<TariffaResponse>();
        Assert.NotNull(tariffa);

        var tariffaDuplicataResponse = await client.PostAsync("tariffe", new
        {
            PrestazioneId = prestazione.Id,
            Regime = Regime.Privato,
            Prezzo = 100
        });
        Assert.Equal(HttpStatusCode.Conflict, tariffaDuplicataResponse.StatusCode);

        client.UseToken(tokenPaziente);
        var creaTariffaComePaziente = await client.PostAsync("tariffe", new
        {
            PrestazioneId = prestazione.Id,
            Regime = Regime.Ssn,
            Prezzo = 0
        });
        Assert.Equal(HttpStatusCode.Forbidden, creaTariffaComePaziente.StatusCode);
        client.UseToken(tokenAdmin);

        // 7. Crea Medico (crea anche l'account Identity associato).
        var emailMedico = $"medico.{suffisso}@medicore.local";
        var medicoResponse = await client.PostAsync("medici", new
        {
            Email = emailMedico,
            Nome = "Medico",
            Cognome = $"Test{suffisso}",
            Specializzazione = "Cardiologia",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.Created, medicoResponse.StatusCode);
        var medicoCreato = await medicoResponse.Content.ReadFromJsonAsync<MedicoCreatoResponse>();
        Assert.NotNull(medicoCreato);
        Assert.False(string.IsNullOrWhiteSpace(medicoCreato!.PasswordGenerata));

        // 7bis. Stessa email medico -> 409. Servizio inesistente -> 400.
        var medicoEmailDuplicataResponse = await client.PostAsync("medici", new
        {
            Email = emailMedico,
            Nome = "Medico",
            Cognome = "Duplicato",
            Specializzazione = "Cardiologia",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.Conflict, medicoEmailDuplicataResponse.StatusCode);

        var medicoServizioInesistenteResponse = await client.PostAsync("medici", new
        {
            Email = $"medico.altro.{suffisso}@medicore.local",
            Nome = "Medico",
            Cognome = "Altro",
            Specializzazione = "Cardiologia",
            ServizioId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.BadRequest, medicoServizioInesistenteResponse.StatusCode);

        // 7ter. Aggiorna Specializzazione/ServizioId del Medico.
        var medicoUpdateResponse = await client.PutAsync($"medici/{medicoCreato.Medico.Id}", new
        {
            Specializzazione = "Cardiologia interventistica",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.NoContent, medicoUpdateResponse.StatusCode);

        var medicoUpdateInesistenteResponse = await client.PutAsync($"medici/{Guid.NewGuid()}", new
        {
            Specializzazione = "x",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.NotFound, medicoUpdateInesistenteResponse.StatusCode);

        client.UseToken(tokenPaziente);
        var medicoUpdateComePazienteResponse = await client.PutAsync($"medici/{medicoCreato.Medico.Id}", new
        {
            Specializzazione = "x",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.Forbidden, medicoUpdateComePazienteResponse.StatusCode);
        client.UseToken(tokenAdmin);

        // 8. Crea Turno per il Medico sulla Prestazione appena creata.
        var turnoResponse = await client.PostAsync("turni", new
        {
            MedicoId = medicoCreato.Medico.Id,
            PrestazioneId = prestazione!.Id,
            GiornoSettimana = GiornoSettimana.Lunedi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 20
        });
        Assert.Equal(HttpStatusCode.Created, turnoResponse.StatusCode);
        var turno = await turnoResponse.Content.ReadFromJsonAsync<TurnoResponse>();
        Assert.NotNull(turno);

        // 9. Un secondo Turno sovrapposto per lo stesso Medico/giorno deve essere rifiutato.
        var turnoSovrappostoResponse = await client.PostAsync("turni", new
        {
            MedicoId = medicoCreato.Medico.Id,
            PrestazioneId = prestazione.Id,
            GiornoSettimana = GiornoSettimana.Lunedi,
            OraInizio = new TimeOnly(10, 0),
            OraFine = new TimeOnly(13, 0),
            DurataSlotMin = 20
        });
        Assert.Equal(HttpStatusCode.Conflict, turnoSovrappostoResponse.StatusCode);

        // 9bis. Turno con OraFine <= OraInizio -> 400. Turno con Medico inesistente -> 400.
        var turnoOrarioNonValidoResponse = await client.PostAsync("turni", new
        {
            MedicoId = medicoCreato.Medico.Id,
            PrestazioneId = prestazione.Id,
            GiornoSettimana = GiornoSettimana.Martedi,
            OraInizio = new TimeOnly(12, 0),
            OraFine = new TimeOnly(9, 0),
            DurataSlotMin = 20
        });
        Assert.Equal(HttpStatusCode.BadRequest, turnoOrarioNonValidoResponse.StatusCode);

        var turnoMedicoInesistenteResponse = await client.PostAsync("turni", new
        {
            MedicoId = Guid.NewGuid(),
            PrestazioneId = prestazione.Id,
            GiornoSettimana = GiornoSettimana.Martedi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 20
        });
        Assert.Equal(HttpStatusCode.BadRequest, turnoMedicoInesistenteResponse.StatusCode);

        // 9ter. Il paziente NON può creare un turno (solo Amministratore).
        client.UseToken(tokenPaziente);
        var creaTurnoComePaziente = await client.PostAsync("turni", new
        {
            MedicoId = medicoCreato.Medico.Id,
            PrestazioneId = prestazione.Id,
            GiornoSettimana = GiornoSettimana.Mercoledi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 20
        });
        Assert.Equal(HttpStatusCode.Forbidden, creaTurnoComePaziente.StatusCode);

        // 10. Il paziente può leggere il turno appena creato (lettura aperta a tutti gli autenticati).
        client.UseToken(tokenPaziente);
        var getTurnoComePaziente = await client.GetAsync($"turni/{turno!.Id}");
        Assert.Equal(HttpStatusCode.OK, getTurnoComePaziente.StatusCode);

        // 11. GET /turni generico (tutti i turni, nessun filtro) è leggibile da qualsiasi autenticato.
        var getTuttiITurniComePaziente = await client.GetAsync("turni");
        Assert.Equal(HttpStatusCode.OK, getTuttiITurniComePaziente.StatusCode);
        var tuttiITurni = await getTuttiITurniComePaziente.Content.ReadFromJsonAsync<List<TurnoResponse>>();
        Assert.Contains(tuttiITurni!, t => t.Id == turno.Id);

        // 12. Il paziente cerca gli slot disponibili per la Prestazione: generazione lazy
        // sulla finestra di 60 giorni a partire dal Turno creato al passo 8 (ogni Lunedì, 9-12, slot da 20 min).
        var getSlotComePazienteResponse = await client.GetAsync($"slot/prestazione/{prestazione!.Id}");
        Assert.Equal(HttpStatusCode.OK, getSlotComePazienteResponse.StatusCode);
        var slotDisponibili = await getSlotComePazienteResponse.Content.ReadFromJsonAsync<List<SlotResponse>>();
        Assert.NotEmpty(slotDisponibili!);
        Assert.All(slotDisponibili!, s => Assert.Equal(turno.Id, s.TurnoId));

        // 12bis. Prestazione inesistente -> 404.
        var getSlotPrestazioneInesistenteResponse = await client.GetAsync($"slot/prestazione/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, getSlotPrestazioneInesistenteResponse.StatusCode);

        // 13. Il paziente prenota il primo slot disponibile.
        var slotPrenotato = slotDisponibili![0];
        var prenotazioneResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotPrenotato.Id,
            Regime = Regime.Ssn
        });
        Assert.Equal(HttpStatusCode.Created, prenotazioneResponse.StatusCode);
        var prenotazione = await prenotazioneResponse.Content.ReadFromJsonAsync<PrenotazioneResponse>();
        Assert.NotNull(prenotazione);
        Assert.Equal(StatoPrenotazione.Confermata, prenotazione!.Stato);

        // 13bis. Lo stesso slot non è più prenotabile: prenotazione duplicata -> 409.
        var prenotazioneDuplicataResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotPrenotato.Id,
            Regime = Regime.Ssn
        });
        Assert.Equal(HttpStatusCode.Conflict, prenotazioneDuplicataResponse.StatusCode);

        // 14. GET /prenotazioni/mie include la prenotazione appena creata.
        var getMieResponse = await client.GetAsync("prenotazioni/mie");
        Assert.Equal(HttpStatusCode.OK, getMieResponse.StatusCode);
        var mie = await getMieResponse.Content.ReadFromJsonAsync<List<PrenotazioneResponse>>();
        Assert.Contains(mie!, p => p.Id == prenotazione.Id);

        // 14bis. GET /prenotazioni/{id} da parte dell'amministratore (non proprietario) -> 200.
        client.UseToken(tokenAdmin);
        var getByIdComeAdminResponse = await client.GetAsync($"prenotazioni/{prenotazione.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdComeAdminResponse.StatusCode);
        client.UseToken(tokenPaziente);

        // 15. Annullamento da parte del paziente proprietario -> 204.
        var annullaResponse = await client.PutAsync<object?>($"prenotazioni/{prenotazione.Id}/annulla", null);
        Assert.Equal(HttpStatusCode.NoContent, annullaResponse.StatusCode);

        // 15bis. Annullare due volte la stessa prenotazione -> 409.
        var annullaDuplicataResponse = await client.PutAsync<object?>($"prenotazioni/{prenotazione.Id}/annulla", null);
        Assert.Equal(HttpStatusCode.Conflict, annullaDuplicataResponse.StatusCode);

        // 15ter. Lo slot annullato torna disponibile per una nuova prenotazione.
        var getSlotDopoAnnullamentoResponse = await client.GetAsync($"slot/prestazione/{prestazione!.Id}");
        var slotDopoAnnullamento = await getSlotDopoAnnullamentoResponse.Content.ReadFromJsonAsync<List<SlotResponse>>();
        Assert.Contains(slotDopoAnnullamento!, s => s.Id == slotPrenotato.Id);

        // 16. SlotId inesistente -> 400.
        var prenotazioneSlotInesistenteResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = Guid.NewGuid(),
            Regime = Regime.Ssn
        });
        Assert.Equal(HttpStatusCode.BadRequest, prenotazioneSlotInesistenteResponse.StatusCode);

        // 17. Login del Medico creato al passo 7 (la prenotazione annullata al passo 15 resta
        // comunque una prenotazione "pregressa" tra questo Medico e questo Paziente).
        var tokenMedico = await client.LoginAsync(emailMedico, medicoCreato.PasswordGenerata);
        Assert.False(string.IsNullOrWhiteSpace(tokenMedico));
        client.UseToken(tokenMedico);

        // 18. Il Paziente NON può creare una Prescrizione (solo Medico).
        client.UseToken(tokenPaziente);
        var creaPrescrizioneComePazienteResponse = await client.PostAsync("prescrizioni", new
        {
            PazienteId = prenotazione.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Test farmaco"
        });
        Assert.Equal(HttpStatusCode.Forbidden, creaPrescrizioneComePazienteResponse.StatusCode);

        // 19. Il Medico crea una Prescrizione per il Paziente con cui ha una prenotazione pregressa.
        client.UseToken(tokenMedico);
        var prescrizioneResponse = await client.PostAsync("prescrizioni", new
        {
            PazienteId = prenotazione.PazienteId,
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Test farmaco"
        });
        Assert.Equal(HttpStatusCode.Created, prescrizioneResponse.StatusCode);
        var prescrizione = await prescrizioneResponse.Content.ReadFromJsonAsync<PrescrizioneResponse>();
        Assert.NotNull(prescrizione);
        Assert.True(prescrizione!.NotificaInviata);

        // 19bis. Paziente senza prenotazioni pregresse con questo Medico -> 400.
        var prescrizioneSenzaStoricoResponse = await client.PostAsync("prescrizioni", new
        {
            PazienteId = Guid.NewGuid(),
            DataEmissione = DateOnly.FromDateTime(DateTime.Now),
            DataScadenza = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Farmaci = "Test farmaco"
        });
        Assert.Equal(HttpStatusCode.BadRequest, prescrizioneSenzaStoricoResponse.StatusCode);

        // 19ter. DataScadenza non successiva a DataEmissione -> 400.
        var oggi = DateOnly.FromDateTime(DateTime.Now);
        var prescrizioneDataNonValidaResponse = await client.PostAsync("prescrizioni", new
        {
            PazienteId = prenotazione.PazienteId,
            DataEmissione = oggi,
            DataScadenza = oggi,
            Farmaci = "Test farmaco"
        });
        Assert.Equal(HttpStatusCode.BadRequest, prescrizioneDataNonValidaResponse.StatusCode);

        // 20. GET /prescrizioni/emesse come Medico autore include la prescrizione appena creata.
        var getEmesseResponse = await client.GetAsync("prescrizioni/emesse");
        Assert.Equal(HttpStatusCode.OK, getEmesseResponse.StatusCode);
        var emesse = await getEmesseResponse.Content.ReadFromJsonAsync<List<PrescrizioneResponse>>();
        Assert.Contains(emesse!, p => p.Id == prescrizione.Id);

        // 21. GET /prescrizioni/mie come Paziente proprietario include la prescrizione.
        client.UseToken(tokenPaziente);
        var getMiePrescrizioniResponse = await client.GetAsync("prescrizioni/mie");
        Assert.Equal(HttpStatusCode.OK, getMiePrescrizioniResponse.StatusCode);
        var miePrescrizioni = await getMiePrescrizioniResponse.Content.ReadFromJsonAsync<List<PrescrizioneResponse>>();
        Assert.Contains(miePrescrizioni!, p => p.Id == prescrizione.Id);

        // 22. GET /prescrizioni/{id} come Paziente proprietario -> 200.
        var getPrescrizioneByIdResponse = await client.GetAsync($"prescrizioni/{prescrizione.Id}");
        Assert.Equal(HttpStatusCode.OK, getPrescrizioneByIdResponse.StatusCode);
    }
}
