namespace MediCore.Api.Domain.Enums;

// Sesso biologico, usato solo nel payload de-identificato inviato all'assistente AI.
// Non è persistito su Paziente: viene derivato dal Codice Fiscale al momento della richiesta.
public enum Sesso
{
    Maschile = 1,
    Femminile = 2
}
