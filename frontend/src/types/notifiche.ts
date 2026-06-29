export const TipoNotifica = {
  PromemoriaAppuntamento: 1,
  Prescrizione: 2,
} as const;

export type TipoNotifica = (typeof TipoNotifica)[keyof typeof TipoNotifica];

export const ETICHETTE_TIPO_NOTIFICA: Record<TipoNotifica, string> = {
  [TipoNotifica.PromemoriaAppuntamento]: "Promemoria",
  [TipoNotifica.Prescrizione]: "Prescrizione",
};

export interface Notifica {
  id: string;
  tipo: TipoNotifica;
  titolo: string;
  messaggio: string;
  riferimentoId: string | null;
  letta: boolean;
  dataCreazione: string;
}
