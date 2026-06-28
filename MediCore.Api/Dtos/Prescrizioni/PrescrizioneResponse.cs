using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Prescrizioni;

public record PrescrizioneResponse
{
    public Guid Id { get; init; }
    public Guid PazienteId { get; init; }
    public string PazienteNomeCompleto { get; init; } = null!;
    public Guid MedicoId { get; init; }
    public string MedicoNomeCompleto { get; init; } = null!;
    public TipoPrescrizione Tipo { get; init; }
    public string? Diagnosi { get; init; }
    public int? DurataGiorni { get; init; }
    public string? Monitoraggio { get; init; }
    public DateOnly DataEmissione { get; init; }
    public DateOnly DataScadenza { get; init; }
    public string? Note { get; init; }
    public bool NotificaInviata { get; init; }
    public bool OriginAssistita { get; init; }
    public IReadOnlyList<RigaPrescrizioneResponse> Righe { get; init; } = [];
}
