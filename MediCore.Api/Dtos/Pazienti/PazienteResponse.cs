namespace MediCore.Api.Dtos.Pazienti;

// Rappresentazione sintetica di un paziente, usata dagli operatori (Amministratore/Medico)
// per selezionarlo in fase di prenotazione. Dati minimi necessari all'identificazione.
public record PazienteResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Cognome { get; init; } = null!;
    public string CodiceFiscale { get; init; } = null!;
}
