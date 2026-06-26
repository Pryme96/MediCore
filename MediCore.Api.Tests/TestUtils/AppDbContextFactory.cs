using MediCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MediCore.Api.Tests.TestUtils;

// Crea un AppDbContext isolato su un database InMemory diverso per ogni test.
public static class AppDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            // Il provider InMemory non supporta le transazioni: BeginTransactionAsync
            // (usata da MedicoService) viene ignorata invece di lanciare un'eccezione.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options, new FakeCurrentUserService());
    }
}
