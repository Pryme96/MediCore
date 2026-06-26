namespace MediCore.Api.Dtos.Medici;

// Risposta alla creazione di un medico: include la password generata, da comunicare una sola volta.
public record MedicoCreatoResponse
{
    public MedicoResponse Medico { get; init; } = null!;
    public string PasswordGenerata { get; init; } = null!;
}
