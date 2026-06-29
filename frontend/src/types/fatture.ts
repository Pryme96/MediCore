import type { Regime } from "./servizi";

export const StatoFattura = {
  Emessa: 1,
  Pagata: 2,
  Scaduta: 3,
  Annullata: 4,
} as const;

export type StatoFattura = (typeof StatoFattura)[keyof typeof StatoFattura];

export const ETICHETTE_STATO_FATTURA: Record<StatoFattura, string> = {
  [StatoFattura.Emessa]: "Emessa",
  [StatoFattura.Pagata]: "Pagata",
  [StatoFattura.Scaduta]: "Scaduta",
  [StatoFattura.Annullata]: "Annullata",
};

export interface Fattura {
  id: string;
  prenotazioneId: string;
  pazienteId: string;
  pazienteNomeCompleto: string;
  importo: number;
  regime: Regime;
  dataEmissione: string;
  stato: StatoFattura;
}
