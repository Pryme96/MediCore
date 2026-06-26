using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediCore.Api.Tests.TestUtils;

// Costruisce un UserManager<AppUser> reale appoggiato sull'AppDbContext di test,
// cosi' i service che usano Identity (es. MedicoService) si possono testare senza mock.
public static class UserManagerFactory
{
    public static UserManager<AppUser> Create(AppDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<UserManager<AppUser>>();
    }
}
