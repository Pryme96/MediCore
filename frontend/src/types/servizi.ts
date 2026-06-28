export interface Servizio {
  id: string;
  nome: string;
  descrizione: string;
}

export interface Prestazione {
  id: string;
  servizioId: string;
  servizioNome: string;
  nome: string;
  descrizione: string;
  durataMinuti: number;
}

export const Regime = {
  Ssn: 1,
  Privato: 2,
  Assicurativo: 3,
} as const;

export type Regime = (typeof Regime)[keyof typeof Regime];

export const ETICHETTE_REGIME: Record<Regime, string> = {
  [Regime.Ssn]: "SSN",
  [Regime.Privato]: "Privato",
  [Regime.Assicurativo]: "Assicurativo",
};

export interface Tariffa {
  id: string;
  prestazioneId: string;
  prestazioneNome: string;
  regime: Regime;
  prezzo: number;
}

export interface ServizioInput {
  nome: string;
  descrizione: string;
}

export interface PrestazioneInput {
  servizioId: string;
  nome: string;
  descrizione: string;
  durataMinuti: number;
}

export interface TariffaInput {
  prestazioneId: string;
  regime: Regime;
  prezzo: number;
}
