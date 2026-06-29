using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Data;

// Contesto EF Core: unisce le tabelle di Identity (AspNet*) e quelle di dominio.
public class AppDbContext : IdentityDbContext<AppUser>
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Paziente> Pazienti => Set<Paziente>();
    public DbSet<Medico> Medici => Set<Medico>();
    public DbSet<Servizio> Servizi => Set<Servizio>();
    public DbSet<Prestazione> Prestazioni => Set<Prestazione>();
    public DbSet<Tariffa> Tariffe => Set<Tariffa>();
    public DbSet<Turno> Turni => Set<Turno>();
    public DbSet<Slot> Slot => Set<Slot>();
    public DbSet<Prenotazione> Prenotazioni => Set<Prenotazione>();
    public DbSet<Referto> Referti => Set<Referto>();
    public DbSet<Prescrizione> Prescrizioni => Set<Prescrizione>();
    public DbSet<RigaPrescrizione> RighePrescrizione => Set<RigaPrescrizione>();
    public DbSet<Fattura> Fatture => Set<Fattura>();
    public DbSet<Notifica> Notifiche => Set<Notifica>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Applica tutte le IEntityTypeConfiguration<T> definite nell'assembly.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    // Valorizza automaticamente i campi di audit sulle entità tracciate.
    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = userId;
            }
        }
    }
}
