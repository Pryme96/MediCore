using System.Security.Cryptography;
using MediCore.Api.Data;
using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Medici;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class MedicoService(AppDbContext db, UserManager<AppUser> userManager) : IMedicoService
{
    public async Task<IReadOnlyList<MedicoResponse>> GetAllAsync() =>
        await Project(db.Medici.AsNoTracking().OrderBy(m => m.User.Cognome)).ToListAsync();

    public async Task<MedicoResponse?> GetByIdAsync(Guid id) =>
        await Project(db.Medici.AsNoTracking().Where(m => m.MedicoId == id))
            .FirstOrDefaultAsync();

    public async Task<(EsitoOperazione Esito, MedicoCreatoResponse? Medico)> CreateAsync(MedicoRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return (EsitoOperazione.Conflitto, null);

        var servizio = await db.Servizi.FirstOrDefaultAsync(s => s.ServizioId == request.ServizioId);
        if (servizio is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        await using var transaction = await db.Database.BeginTransactionAsync();

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            Nome = request.Nome,
            Cognome = request.Cognome
        };

        var password = GeneraPassword();
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return (EsitoOperazione.DatiNonValidi, null);

        await userManager.AddToRoleAsync(user, AppRoles.Medico);

        var medico = new Medico
        {
            UserId = user.Id,
            Specializzazione = request.Specializzazione,
            ServizioId = request.ServizioId
        };
        db.Medici.Add(medico);
        await db.SaveChangesAsync();

        await transaction.CommitAsync();

        var risposta = new MedicoCreatoResponse
        {
            Medico = ToResponse(medico, user, servizio.Nome),
            PasswordGenerata = password
        };

        return (EsitoOperazione.Ok, risposta);
    }

    public async Task<EsitoOperazione> UpdateAsync(Guid id, MedicoUpdateRequest request)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.MedicoId == id);
        if (medico is null)
            return EsitoOperazione.NonTrovato;

        var servizioEsiste = await db.Servizi.AnyAsync(s => s.ServizioId == request.ServizioId);
        if (!servizioEsiste)
            return EsitoOperazione.RiferimentoNonValido;

        medico.Specializzazione = request.Specializzazione;
        medico.ServizioId = request.ServizioId;
        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    public async Task<(EsitoOperazione Esito, PasswordResetResponse? Risultato)> ResetPasswordAsync(Guid id)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.MedicoId == id);
        if (medico is null)
            return (EsitoOperazione.NonTrovato, null);

        var user = await userManager.FindByIdAsync(medico.UserId);
        if (user is null)
            return (EsitoOperazione.NonTrovato, null);

        var password = GeneraPassword();
        await userManager.RemovePasswordAsync(user);
        var result = await userManager.AddPasswordAsync(user, password);
        if (!result.Succeeded)
            return (EsitoOperazione.DatiNonValidi, null);

        return (EsitoOperazione.Ok, new PasswordResetResponse { PasswordGenerata = password });
    }

    // Password temporanea conforme alle regole di Identity (maiuscola, minuscola, cifra, carattere speciale, min 8 caratteri).
    private static string GeneraPassword()
    {
        const string speciali = "!@#$%?";
        var bytes = RandomNumberGenerator.GetBytes(6);
        var corpo = Convert.ToBase64String(bytes).Replace("+", "A").Replace("/", "a").Replace("=", "");
        var speciale = speciali[RandomNumberGenerator.GetInt32(speciali.Length)];
        return $"Aa1{speciale}{corpo}";
    }

    private static IQueryable<MedicoResponse> Project(IQueryable<Medico> query) =>
        query.Select(m => new MedicoResponse
        {
            Id = m.MedicoId,
            Email = m.User.Email!,
            Nome = m.User.Nome,
            Cognome = m.User.Cognome,
            Specializzazione = m.Specializzazione,
            ServizioId = m.ServizioId,
            ServizioNome = m.Servizio.Nome
        });

    private static MedicoResponse ToResponse(Medico m, AppUser user, string servizioNome) => new()
    {
        Id = m.MedicoId,
        Email = user.Email!,
        Nome = user.Nome,
        Cognome = user.Cognome,
        Specializzazione = m.Specializzazione,
        ServizioId = m.ServizioId,
        ServizioNome = servizioNome
    };
}
