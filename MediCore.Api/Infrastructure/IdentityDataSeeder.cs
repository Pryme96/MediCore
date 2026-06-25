using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediCore.Api.Infrastructure;

// Inizializzazione all'avvio: crea i ruoli applicativi e l'amministratore di default.
public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminSection = configuration.GetSection("SeedAdmin");
        var adminEmail = adminSection["Email"];
        var adminPassword = adminSection["Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Nome = "Amministratore",
            Cognome = "Sistema"
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, AppRoles.Amministratore);
    }
}
