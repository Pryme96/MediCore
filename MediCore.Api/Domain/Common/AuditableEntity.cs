namespace MediCore.Api.Domain.Common;

// Classe base per le entità di dominio tracciabili.
// I campi di audit vengono valorizzati automaticamente in AppDbContext.SaveChangesAsync().
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
