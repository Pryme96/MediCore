using MediCore.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MediCore.Api.Data;

// Factory usata solo a design-time (Add-Migration / Update-Database):
// costruisce il DbContext senza dipendere dall'host dell'applicazione.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=medicore.db")
            .Options;

        return new AppDbContext(options, new DesignTimeCurrentUserService());
    }

    // A design-time non esiste un utente autenticato: implementazione neutra.
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public string? UserId => null;
        public string? UserName => null;
    }
}
