namespace MediCore.Api.Domain.Enums;

// Stato di una prenotazione. Nasce Confermata perché l'occupazione dello slot è immediata.
public enum StatoPrenotazione
{
    Confermata = 1,
    Annullata = 2,
    Completata = 3,
    NonPresentato = 4
}
