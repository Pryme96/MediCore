namespace MediCore.Api.Domain.Enums;

// Esito dell'invio di una notifica tramite il canale configurato (INotificationSender).
public enum StatoInvioNotifica
{
    InAttesa = 1,
    Inviata = 2,
    Fallita = 3
}
