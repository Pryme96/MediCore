using Microsoft.AspNetCore.Identity;

namespace MediCore.Api.Domain.Entities;

// Utente applicativo: estende l'utente di Identity con i dati anagrafici di base.
public class AppUser : IdentityUser
{
    public string Nome { get; set; } = null!;
    public string Cognome { get; set; } = null!;

    // Profilo di dominio collegato: un utente è paziente oppure medico (o solo amministratore).
    public Paziente? Paziente { get; set; }
    public Medico? Medico { get; set; }
}
