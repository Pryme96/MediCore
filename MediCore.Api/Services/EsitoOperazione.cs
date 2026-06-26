namespace MediCore.Api.Services;

// Esito generico di un'operazione di scrittura, mappato dai controller su status code HTTP.
public enum EsitoOperazione
{
    Ok,
    NonTrovato,
    RiferimentoNonValido,
    Conflitto
}
