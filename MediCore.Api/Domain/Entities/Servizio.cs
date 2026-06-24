using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Area clinica dell'ambulatorio (es. Cardiologia, Ortopedia).
public class Servizio : AuditableEntity
{
    public Guid ServizioId { get; set; } = Guid.CreateVersion7();
    public string Nome { get; set; } = null!;
    public string Descrizione { get; set; } = null!;

    public ICollection<Prestazione> Prestazioni { get; set; } = new List<Prestazione>();
    public ICollection<Medico> Medici { get; set; } = new List<Medico>();
}
