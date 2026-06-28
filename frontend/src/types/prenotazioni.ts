import type { Regime } from "./servizi";

export interface Slot {
  id: string;
  turnoId: string;
  medicoId: string;
  medicoNomeCompleto: string;
  dataOraInizio: string;
  dataOraFine: string;
}

export const StatoPrenotazione = {
  Confermata: 1,
  Annullata: 2,
  Completata: 3,
  NonPresentato: 4,
} as const;

export type StatoPrenotazione = (typeof StatoPrenotazione)[keyof typeof StatoPrenotazione];

export const ETICHETTE_STATO_PRENOTAZIONE: Record<StatoPrenotazione, string> = {
  [StatoPrenotazione.Confermata]: "Confermata",
  [StatoPrenotazione.Annullata]: "Annullata",
  [StatoPrenotazione.Completata]: "Completata",
  [StatoPrenotazione.NonPresentato]: "Non presentato",
};

export interface Prenotazione {
  id: string;
  pazienteId: string;
  pazienteNomeCompleto: string;
  slotId: string;
  medicoNomeCompleto: string;
  prestazioneNome: string;
  dataOraInizio: string;
  dataOraFine: string;
  regime: Regime;
  stato: StatoPrenotazione;
  note: string | null;
}

export interface PrenotazioneInput {
  slotId: string;
  regime: Regime;
  note?: string;
  // Valorizzato solo quando un operatore (Amministratore/Medico) prenota per un paziente.
  pazienteId?: string;
}
