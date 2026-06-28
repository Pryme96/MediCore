using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Singola riga di una prescrizione: un farmaco con la relativa posologia e quantità.
public class RigaPrescrizione : AuditableEntity
{
    public Guid RigaPrescrizioneId { get; set; } = Guid.CreateVersion7();
    public Guid PrescrizioneId { get; set; }
    public string Farmaco { get; set; } = null!;
    public string Posologia { get; set; } = null!;
    public int Quantita { get; set; }

    public Prescrizione Prescrizione { get; set; } = null!;
}
