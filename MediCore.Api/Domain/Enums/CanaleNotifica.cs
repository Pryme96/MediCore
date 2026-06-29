namespace MediCore.Api.Domain.Enums;

// Canale di consegna della notifica. In sviluppo si usa solo InApp (centro notifiche +
// log); Email/Sms sono evoluzione futura dietro la stessa astrazione INotificationSender.
public enum CanaleNotifica
{
    InApp = 1,
    Email = 2,
    Sms = 3
}
