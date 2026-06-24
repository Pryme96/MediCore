namespace MediCore.Api.Services;

// Espone l'identità dell'utente corrente, usata per valorizzare i campi di audit.
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}
