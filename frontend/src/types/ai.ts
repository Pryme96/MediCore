import type { TipoPrescrizione } from "./prescrizioni";

export const Sesso = {
  Maschile: 1,
  Femminile: 2,
} as const;

export type Sesso = (typeof Sesso)[keyof typeof Sesso];

export interface SuggerimentoRequest {
  pazienteId: string;
  tipo: TipoPrescrizione;
  contestoClinico: string;
  allergie?: string;
  terapieInCorso?: string;
}

export interface RigaSuggerita {
  farmaco: string;
  posologia: string;
  quantita: number;
}

export interface SuggerimentoOpzione {
  righe: RigaSuggerita[];
  diagnosiSuggerita: string | null;
  durataGiorni: number | null;
  monitoraggio: string | null;
  motivazione: string;
  avvertenze: string | null;
}

// Payload de-identificato realmente inviato all'assistente (echo, per trasparenza):
// nessun nome, codice fiscale, data di nascita o identificativo del paziente/medico.
export interface DatiClinici {
  tipo: TipoPrescrizione;
  eta: number;
  sesso: Sesso | null;
  contestoClinico: string;
  allergie: string | null;
  terapieInCorso: string | null;
}

export interface SuggerimentoResponse {
  opzioni: SuggerimentoOpzione[];
  datiInviati: DatiClinici;
  demo: boolean;
}
