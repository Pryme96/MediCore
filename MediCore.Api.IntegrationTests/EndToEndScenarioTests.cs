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
        // Password corrente del medico: aggiornata dal reset al passo 7quater e riusata al login (passo 17).
        var passwordMedico = medicoCreato.PasswordGenerata;

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

        // 7quater. Reset password del Medico: l'Amministratore rigenera una password temporanea.
        var resetPasswordResponse = await client.PostAsync($"medici/{medicoCreato.Medico.Id}/reset-password", new { });
        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);
        var resetPassword = await resetPasswordResponse.Content.ReadFromJsonAsync<PasswordResetResponse>();
        Assert.NotNull(resetPassword);
        Assert.False(string.IsNullOrWhiteSpace(resetPassword!.PasswordGenerata));
        Assert.NotEqual(passwordMedico, resetPassword.PasswordGenerata);
        // Da qui in poi vale la nuova password (verificata al login del passo 17).
        passwordMedico = resetPassword.PasswordGenerata;

        var resetPasswordInesistenteResponse = await client.PostAsync($"medici/{Guid.NewGuid()}/reset-password", new { });
        Assert.Equal(HttpStatusCode.NotFound, resetPasswordInesistenteResponse.StatusCode);

        client.UseToken(tokenPaziente);
        var medicoUpdateComePazienteResponse = await client.PutAsync($"medici/{medicoCreato.Medico.Id}", new
        {
            Specializzazione = "x",
            ServizioId = servizio.Id
        });
        Assert.Equal(HttpStatusCode.Forbidden, medicoUpdateComePazienteResponse.StatusCode);

        // Il Paziente non può resettare la password di un medico.
        var resetPasswordComePazienteResponse = await client.PostAsync($"medici/{medicoCreato.Medico.Id}/reset-password", new { });
        Assert.Equal(HttpStatusCode.Forbidden, resetPasswordComePazienteResponse.StatusCode);
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

        // 15quater. Lo slot liberato può essere effettivamente riprenotato (regressione: prima
        // la riga annullata residua violava l'indice unico su SlotId e l'API restituiva 500).
        var riprenotaSlotResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotPrenotato.Id,
            Regime = Regime.Ssn
        });
        Assert.Equal(HttpStatusCode.Created, riprenotaSlotResponse.StatusCode);

        // 16. SlotId inesistente -> 400.
        var prenotazioneSlotInesistenteResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = Guid.NewGuid(),
            Regime = Regime.Ssn
        });
        Assert.Equal(HttpStatusCode.BadRequest, prenotazioneSlotInesistenteResponse.StatusCode);

        // 17. Login del Medico creato al passo 7 (la prenotazione annullata al passo 15 resta
        // comunque una prenotazione "pregressa" tra questo Medico e questo Paziente).
        var tokenMedico = await client.LoginAsync(emailMedico, passwordMedico);
        Assert.False(string.IsNullOrWhiteSpace(tokenMedico));
        client.UseToken(tokenMedico);

        // 17bis. GET /turni/miei: il Medico vede i propri turni (incluso quello creato al passo 8).
        var turniMieiResponse = await client.GetAsync("turni/miei");
        Assert.Equal(HttpStatusCode.OK, turniMieiResponse.StatusCode);
        var turniMiei = await turniMieiResponse.Content.ReadFromJsonAsync<List<TurnoResponse>>();
        Assert.NotNull(turniMiei);
        Assert.Contains(turniMiei!, t => t.MedicoId == medicoCreato.Medico.Id);

        // Il Paziente non può accedere a /turni/miei (solo Medico).
        client.UseToken(tokenPaziente);
        var turniMieiComePazienteResponse = await client.GetAsync("turni/miei");
        Assert.Equal(HttpStatusCode.Forbidden, turniMieiComePazienteResponse.StatusCode);
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

        // 23. Il Paziente NON può caricare un Referto (solo Medico).
        var pdfFinto = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 contenuto finto");
        var uploadRefertoComePazienteResponse = await client.PostFileAsync(
            "referti", prenotazione.Id, pdfFinto, "referto.pdf", "application/pdf");
        Assert.Equal(HttpStatusCode.Forbidden, uploadRefertoComePazienteResponse.StatusCode);

        // 24. Il Medico carica un Referto PDF sulla propria Prenotazione.
        client.UseToken(tokenMedico);
        var uploadRefertoResponse = await client.PostFileAsync(
            "referti", prenotazione.Id, pdfFinto, "referto.pdf", "application/pdf", "Esito nella norma");
        Assert.Equal(HttpStatusCode.Created, uploadRefertoResponse.StatusCode);
        var referto = await uploadRefertoResponse.Content.ReadFromJsonAsync<RefertoResponse>();
        Assert.NotNull(referto);

        // 24bis. File non PDF -> 400.
        var uploadRefertoNonPdfResponse = await client.PostFileAsync(
            "referti", prenotazione.Id, pdfFinto, "referto.png", "image/png");
        Assert.Equal(HttpStatusCode.BadRequest, uploadRefertoNonPdfResponse.StatusCode);

        // 24ter. Prenotazione inesistente -> 404.
        var uploadRefertoPrenotazioneInesistenteResponse = await client.PostFileAsync(
            "referti", Guid.NewGuid(), pdfFinto, "referto.pdf", "application/pdf");
        Assert.Equal(HttpStatusCode.NotFound, uploadRefertoPrenotazioneInesistenteResponse.StatusCode);

        // 25. GET /referti/{id} come Medico autore -> 200.
        var getRefertoByIdResponse = await client.GetAsync($"referti/{referto!.Id}");
        Assert.Equal(HttpStatusCode.OK, getRefertoByIdResponse.StatusCode);

        // 26. GET /referti/prenotazione/{id} come Paziente proprietario -> 200.
        client.UseToken(tokenPaziente);
        var getRefertoByPrenotazioneResponse = await client.GetAsync($"referti/prenotazione/{prenotazione.Id}");
        Assert.Equal(HttpStatusCode.OK, getRefertoByPrenotazioneResponse.StatusCode);

        // 27. GET /referti/{id}/file come Paziente proprietario -> 200, contenuto PDF scaricato.
        var downloadRefertoResponse = await client.GetAsync($"referti/{referto.Id}/file");
        Assert.Equal(HttpStatusCode.OK, downloadRefertoResponse.StatusCode);
        var fileScaricato = await downloadRefertoResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(pdfFinto, fileScaricato);

        // 27bis. Un secondo Medico estraneo alla prenotazione non può scaricare il file.
        client.UseToken(tokenAdmin);
        var creaAltroMedicoResponse = await client.PostAsync("medici", new
        {
            Email = $"medico.estraneo.{suffisso}@medicore.local",
            Nome = "Estraneo",
            Cognome = "AlReferto",
            Specializzazione = "Ortopedia",
            ServizioId = servizio.Id
        });
        var altroMedicoCreato = await creaAltroMedicoResponse.Content.ReadFromJsonAsync<MedicoCreatoResponse>();
        var tokenMedicoEstraneo = await client.LoginAsync(altroMedicoCreato!.Medico.Email, altroMedicoCreato.PasswordGenerata);
        client.UseToken(tokenMedicoEstraneo);
        var downloadRefertoComeEstraneoResponse = await client.GetAsync($"referti/{referto.Id}/file");
        Assert.Equal(HttpStatusCode.Forbidden, downloadRefertoComeEstraneoResponse.StatusCode);

        // 28. Un secondo upload sulla stessa Prenotazione sovrascrive il referto precedente
        // (stesso Id, nuovo contenuto): qui verifichiamo solo che resti un'unica risorsa.
        client.UseToken(tokenMedico);
        var pdfAggiornato = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 versione corretta");
        var secondoUploadResponse = await client.PostFileAsync(
            "referti", prenotazione.Id, pdfAggiornato, "referto-v2.pdf", "application/pdf", "Esito corretto");
        Assert.Equal(HttpStatusCode.Created, secondoUploadResponse.StatusCode);
        var refertoAggiornato = await secondoUploadResponse.Content.ReadFromJsonAsync<RefertoResponse>();
        Assert.Equal(referto.Id, refertoAggiornato!.Id);
        Assert.Equal("Esito corretto", refertoAggiornato.Contenuto);

        // 29. Il Paziente prenota un secondo slot, con Regime Privato (tariffa creata al passo 6ter).
        client.UseToken(tokenPaziente);
        var secondoSlot = slotDisponibili[1];
        var secondaPrenotazioneResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = secondoSlot.Id,
            Regime = Regime.Privato
        });
        Assert.Equal(HttpStatusCode.Created, secondaPrenotazioneResponse.StatusCode);
        var secondaPrenotazione = await secondaPrenotazioneResponse.Content.ReadFromJsonAsync<PrenotazioneResponse>();
        Assert.NotNull(secondaPrenotazione);

        // 30. Il Paziente NON può completare la propria Prenotazione (solo Medico titolare o Amministratore).
        var completaComePazienteResponse = await client.PutAsync<object?>($"prenotazioni/{secondaPrenotazione!.Id}/completa", null);
        Assert.Equal(HttpStatusCode.Forbidden, completaComePazienteResponse.StatusCode);

        // 31. Il Medico titolare completa la Prenotazione: viene generata la Fattura dalla Tariffa Privato/80.
        client.UseToken(tokenMedico);
        var completaResponse = await client.PutAsync<object?>($"prenotazioni/{secondaPrenotazione.Id}/completa", null);
        Assert.Equal(HttpStatusCode.NoContent, completaResponse.StatusCode);

        // 31bis. Completare due volte la stessa Prenotazione -> 409.
        var completaDuplicataResponse = await client.PutAsync<object?>($"prenotazioni/{secondaPrenotazione.Id}/completa", null);
        Assert.Equal(HttpStatusCode.Conflict, completaDuplicataResponse.StatusCode);

        // 32. GET /fatture/mie come Paziente include la fattura generata con l'importo della Tariffa.
        client.UseToken(tokenPaziente);
        var getMieFattureResponse = await client.GetAsync("fatture/mie");
        Assert.Equal(HttpStatusCode.OK, getMieFattureResponse.StatusCode);
        var mieFatture = await getMieFattureResponse.Content.ReadFromJsonAsync<List<FatturaResponse>>();
        var fatturaGenerata = mieFatture!.Single(f => f.PrenotazioneId == secondaPrenotazione.Id);
        Assert.Equal(80, fatturaGenerata.Importo);

        // 33. GET /fatture/{id} come Medico titolare -> 200.
        client.UseToken(tokenMedico);
        var getFatturaComeMedicoResponse = await client.GetAsync($"fatture/{fatturaGenerata.Id}");
        Assert.Equal(HttpStatusCode.OK, getFatturaComeMedicoResponse.StatusCode);

        // 34. Prenotazione con Regime senza Tariffa configurata (Assicurativo): il completamento -> 400.
        client.UseToken(tokenPaziente);
        var terzaPrenotazioneResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotDisponibili[2].Id,
            Regime = Regime.Assicurativo
        });
        var terzaPrenotazione = await terzaPrenotazioneResponse.Content.ReadFromJsonAsync<PrenotazioneResponse>();
        client.UseToken(tokenMedico);
        var completaSenzaTariffaResponse = await client.PutAsync<object?>($"prenotazioni/{terzaPrenotazione!.Id}/completa", null);
        Assert.Equal(HttpStatusCode.BadRequest, completaSenzaTariffaResponse.StatusCode);

        // 35. GET /pazienti è riservato agli operatori (Amministratore/Medico) -> 200; Paziente -> 403.
        client.UseToken(tokenAdmin);
        var getPazientiComeAdminResponse = await client.GetAsync("pazienti");
        Assert.Equal(HttpStatusCode.OK, getPazientiComeAdminResponse.StatusCode);
        var pazienti = await getPazientiComeAdminResponse.Content.ReadFromJsonAsync<List<PazienteResponse>>();
        Assert.Contains(pazienti!, p => p.Id == prenotazione.PazienteId);

        client.UseToken(tokenMedico);
        var getPazientiComeMedicoResponse = await client.GetAsync("pazienti");
        Assert.Equal(HttpStatusCode.OK, getPazientiComeMedicoResponse.StatusCode);

        client.UseToken(tokenPaziente);
        var getPazientiComePazienteResponse = await client.GetAsync("pazienti");
        Assert.Equal(HttpStatusCode.Forbidden, getPazientiComePazienteResponse.StatusCode);

        // 36. Il Medico prenota una visita per conto del paziente (PazienteId nel body) -> 201.
        client.UseToken(tokenMedico);
        var prenotazionePerPazienteResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotDisponibili[3].Id,
            Regime = Regime.Ssn,
            PazienteId = prenotazione.PazienteId
        });
        Assert.Equal(HttpStatusCode.Created, prenotazionePerPazienteResponse.StatusCode);
        var prenotazionePerPaziente = await prenotazionePerPazienteResponse.Content.ReadFromJsonAsync<PrenotazioneResponse>();
        Assert.Equal(prenotazione.PazienteId, prenotazionePerPaziente!.PazienteId);

        // 36bis. Anche l'Amministratore può prenotare per un paziente -> 201.
        client.UseToken(tokenAdmin);
        var prenotazioneAdminPerPazienteResponse = await client.PostAsync("prenotazioni", new
        {
            SlotId = slotDisponibili[4].Id,
            Regime = Regime.Ssn,
            PazienteId = prenotazione.PazienteId
        });
        Assert.Equal(HttpStatusCode.Created, prenotazioneAdminPerPazienteResponse.StatusCode);

        // 37. Il Medico vede l'agenda delle prenotazioni sui propri turni e ne legge una per id.
        client.UseToken(tokenMedico);
        var agendaResponse = await client.GetAsync("prenotazioni/agenda");
        Assert.Equal(HttpStatusCode.OK, agendaResponse.StatusCode);
        var agenda = await agendaResponse.Content.ReadFromJsonAsync<List<PrenotazioneResponse>>();
        Assert.Contains(agenda!, p => p.Id == prenotazionePerPaziente!.Id);

        var getByIdComeMedicoTitolareResponse = await client.GetAsync($"prenotazioni/{prenotazionePerPaziente!.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdComeMedicoTitolareResponse.StatusCode);

        // 37bis. Il Paziente non può accedere all'agenda del medico.
        client.UseToken(tokenPaziente);
        var agendaComePazienteResponse = await client.GetAsync("prenotazioni/agenda");
        Assert.Equal(HttpStatusCode.Forbidden, agendaComePazienteResponse.StatusCode);

        // 38. Il Medico titolare può annullare una prenotazione sul proprio turno.
        client.UseToken(tokenMedico);
        var annullaComeMedicoResponse = await client.PutAsync<object?>($"prenotazioni/{prenotazionePerPaziente.Id}/annulla", null);
        Assert.Equal(HttpStatusCode.NoContent, annullaComeMedicoResponse.StatusCode);

        // 39. GET /prenotazioni (elenco completo) è riservato all'Amministratore.
        client.UseToken(tokenAdmin);
        var getTutteResponse = await client.GetAsync("prenotazioni");
        Assert.Equal(HttpStatusCode.OK, getTutteResponse.StatusCode);
        var tutte = await getTutteResponse.Content.ReadFromJsonAsync<List<PrenotazioneResponse>>();
        Assert.Contains(tutte!, p => p.Id == prenotazione.Id);

        client.UseToken(tokenMedico);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("prenotazioni")).StatusCode);
        client.UseToken(tokenPaziente);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("prenotazioni")).StatusCode);
    }
}
