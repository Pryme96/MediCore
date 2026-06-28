export const GiornoSettimana = {
  Lunedi: 1,
  Martedi: 2,
  Mercoledi: 3,
  Giovedi: 4,
  Venerdi: 5,
  Sabato: 6,
  Domenica: 7,
} as const;

export type GiornoSettimana = (typeof GiornoSettimana)[keyof typeof GiornoSettimana];

export const ETICHETTE_GIORNO: Record<GiornoSettimana, string> = {
  [GiornoSettimana.Lunedi]: "Lunedì",
  [GiornoSettimana.Martedi]: "Martedì",
  [GiornoSettimana.Mercoledi]: "Mercoledì",
  [GiornoSettimana.Giovedi]: "Giovedì",
  [GiornoSettimana.Venerdi]: "Venerdì",
  [GiornoSettimana.Sabato]: "Sabato",
  [GiornoSettimana.Domenica]: "Domenica",
};

export interface Turno {
  id: string;
  medicoId: string;
  medicoNomeCompleto: string;
  prestazioneId: string;
  prestazioneNome: string;
  giornoSettimana: GiornoSettimana;
  oraInizio: string;
  oraFine: string;
  durataSlotMin: number;
}

export interface TurnoInput {
  medicoId: string;
  prestazioneId: string;
  giornoSettimana: GiornoSettimana;
  oraInizio: string;
  oraFine: string;
  durataSlotMin: number;
}
