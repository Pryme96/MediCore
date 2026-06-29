namespace MediCore.Api.Domain.Enums;

// Stato di una prenotazione. Nasce Confermata perché l'occupazione dello slot è immediata.
public enum StatoPrenotazione
{
    Confermata = 1,
    Annullata = 2,
    Completata = 3,
    NonPresentato = 4,
    // Visita erogata dal medico, in attesa di fatturazione da parte dell'amministratore.
    Erogata = 5
}
