using System.Security.Claims;
using MediCore.Api.Data;
using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Auth;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    UserManager<AppUser> userManager,
    ITokenService tokenService,
    AppDbContext db) : ControllerBase
{
    // Auto-registrazione di un paziente: crea l'account, assegna il ruolo e il profilo.
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Conflict("Esiste già un account con questa email.");

        await using var transaction = await db.Database.BeginTransactionAsync();

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            Nome = request.Nome,
            Cognome = request.Cognome
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, AppRoles.Paziente);

        var paziente = new Paziente
        {
            UserId = user.Id,
            CodiceFiscale = request.CodiceFiscale,
            DataNascita = request.DataNascita,
            Telefono = request.Telefono
        };
        db.Pazienti.Add(paziente);
        await db.SaveChangesAsync();

        await transaction.CommitAsync();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(BuildAuthResponse(user, roles));
    }

    // Verifica le credenziali ed emette il token di accesso.
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Credenziali non valide.");

        var roles = await userManager.GetRolesAsync(user);
        return Ok(BuildAuthResponse(user, roles));
    }

    // Restituisce i dati dell'utente autenticato.
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfoResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserInfoResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Nome = user.Nome,
            Cognome = user.Cognome,
            Ruoli = roles
        });
    }

    private AuthResponse BuildAuthResponse(AppUser user, IList<string> roles)
    {
        var token = tokenService.CreateToken(user, roles);
        return new AuthResponse
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Email = user.Email!,
            Ruoli = roles
        };
    }
}
