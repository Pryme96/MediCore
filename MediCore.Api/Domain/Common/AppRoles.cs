namespace MediCore.Api.Domain.Common;

// Nomi dei ruoli applicativi, centralizzati per evitare stringhe sparse nel codice.
public static class AppRoles
{
    public const string Paziente = "Paziente";
    public const string Medico = "Medico";
    public const string Amministratore = "Amministratore";

    public static readonly string[] All = { Paziente, Medico, Amministratore };
}
