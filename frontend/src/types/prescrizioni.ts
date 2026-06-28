export const TipoPrescrizione = {
  Farmacologica: 1,
  PianoTerapeutico: 2,
} as const;

export type TipoPrescrizione = (typeof TipoPrescrizione)[keyof typeof TipoPrescrizione];

export const ETICHETTE_TIPO_PRESCRIZIONE: Record<TipoPrescrizione, string> = {
  [TipoPrescrizione.Farmacologica]: "Farmacologica",
  [TipoPrescrizione.PianoTerapeutico]: "Piano terapeutico",
};

export interface RigaPrescrizione {
  farmaco: string;
  posologia: string;
  quantita: number;
}

export interface Prescrizione {
  id: string;
  pazienteId: string;
  pazienteNomeCompleto: string;
  medicoId: string;
  medicoNomeCompleto: string;
  tipo: TipoPrescrizione;
  diagnosi: string | null;
  durataGiorni: number | null;
  monitoraggio: string | null;
  dataEmissione: string;
  dataScadenza: string;
  note: string | null;
  notificaInviata: boolean;
  righe: RigaPrescrizione[];
}

export interface PrescrizioneInput {
  pazienteId: string;
  tipo: TipoPrescrizione;
  diagnosi?: string;
  durataGiorni?: number;
  monitoraggio?: string;
  dataEmissione: string;
  dataScadenza: string;
  note?: string;
  righe: RigaPrescrizione[];
}
