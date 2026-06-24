namespace MediCore.Api.Domain.Enums;

// Stato di una fattura, usato anche per gli indicatori della dashboard finanziaria.
public enum StatoFattura
{
    Emessa = 1,
    Pagata = 2,
    Scaduta = 3,
    Annullata = 4
}
