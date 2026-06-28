namespace MediCore.Api.Dtos.Ai;

// Una delle opzioni proposte dall'assistente. Motivazione/Avvertenze sono supporto decisionale
// (mostrate sulla card, non salvate sulla prescrizione). I campi Diagnosi/Durata/Monitoraggio
// sono valorizzati solo per il Piano Terapeutico.
public record SuggerimentoOpzione
{
    public IReadOnlyList<RigaSuggerita> Righe { get; init; } = [];
    public string? DiagnosiSuggerita { get; init; }
    public int? DurataGiorni { get; init; }
    public string? Monitoraggio { get; init; }
    public string Motivazione { get; init; } = null!;
    public string? Avvertenze { get; init; }
}
