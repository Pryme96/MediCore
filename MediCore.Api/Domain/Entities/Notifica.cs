using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Notifica diretta a un paziente: promemoria di un appuntamento imminente o avviso di una
// nuova prescrizione. È persistita (centro notifiche in-app) e tracciata nell'esito di invio;
// il canale fisico (email/SMS) è astratto da INotificationSender.
public class Notifica : AuditableEntity
{
    public Guid NotificaId { get; set; } = Guid.CreateVersion7();
    public string DestinatarioUserId { get; set; } = null!;
    public TipoNotifica Tipo { get; set; }
    public string Titolo { get; set; } = null!;
    public string Messaggio { get; set; } = null!;
    // Riferimento all'entità di origine (PrenotazioneId o PrescrizioneId), per il collegamento
    // e per evitare la generazione di promemoria duplicati sullo stesso appuntamento.
    public Guid? RiferimentoId { get; set; }
    public bool Letta { get; set; }
    public StatoInvioNotifica StatoInvio { get; set; } = StatoInvioNotifica.InAttesa;
    public DateTime? DataInvio { get; set; }
    public CanaleNotifica Canale { get; set; } = CanaleNotifica.InApp;

    public AppUser Destinatario { get; set; } = null!;
}
