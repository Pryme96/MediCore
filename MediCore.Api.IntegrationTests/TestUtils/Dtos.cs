namespace MediCore.Api.IntegrationTests.TestUtils;

// Copie locali minime dei contratti API: il progetto non referenzia MediCore.Api
// di proposito, i test dialogano con il server solo via HTTP, come farebbe un client esterno.

public enum GiornoSettimana
{
    Lunedi = 1,
    Martedi = 2,
    Mercoledi = 3,
    Giovedi = 4,
    Venerdi = 5,
    Sabato = 6,
    Domenica = 7
}

public enum Regime
{
    Ssn = 1,
    Privato = 2,
    Assicurativo = 3
}

public record ServizioResponse(Guid Id, string Nome, string Descrizione);

public record TariffaResponse(Guid Id, Guid PrestazioneId, string PrestazioneNome, Regime Regime, decimal Prezzo);

public record UserInfoResponse(string Id, string Email, string Nome, string Cognome, IReadOnlyList<string> Ruoli);

public record PrestazioneResponse(Guid Id, Guid ServizioId, string ServizioNome, string Nome, string Descrizione, int DurataMinuti);

public record MedicoResponse(Guid Id, string Email, string Nome, string Cognome, string Specializzazione, Guid ServizioId, string ServizioNome);

public record MedicoCreatoResponse(MedicoResponse Medico, string PasswordGenerata);

public record TurnoResponse(
    Guid Id,
    Guid MedicoId,
    string MedicoNomeCompleto,
    Guid PrestazioneId,
    string PrestazioneNome,
    GiornoSettimana GiornoSettimana,
    TimeOnly OraInizio,
    TimeOnly OraFine,
    int DurataSlotMin);

public record SlotResponse(
    Guid Id,
    Guid TurnoId,
    Guid MedicoId,
    string MedicoNomeCompleto,
    DateTime DataOraInizio,
    DateTime DataOraFine);

public enum StatoPrenotazione
{
    Confermata = 1,
    Annullata = 2,
    Completata = 3,
    NonPresentato = 4
}

public record PrenotazioneResponse(
    Guid Id,
    Guid PazienteId,
    string PazienteNomeCompleto,
    Guid SlotId,
    string MedicoNomeCompleto,
    string PrestazioneNome,
    DateTime DataOraInizio,
    DateTime DataOraFine,
    Regime Regime,
    StatoPrenotazione Stato,
    string? Note);
