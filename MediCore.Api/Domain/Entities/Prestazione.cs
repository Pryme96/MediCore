using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Prestazione erogabile nell'ambito di un servizio.
public class Prestazione : AuditableEntity
{
    public Guid PrestazioneId { get; set; } = Guid.CreateVersion7();
    public Guid ServizioId { get; set; }
    public string Nome { get; set; } = null!;
    public string Descrizione { get; set; } = null!;
    public int DurataMinuti { get; set; }

    public Servizio Servizio { get; set; } = null!;
    public ICollection<Tariffa> Tariffe { get; set; } = new List<Tariffa>();
    public ICollection<Turno> Turni { get; set; } = new List<Turno>();
}
