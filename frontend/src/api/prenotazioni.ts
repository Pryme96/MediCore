import apiClient from "./client";
import type { Prenotazione, PrenotazioneInput, Slot } from "../types/prenotazioni";

export const getSlotPerPrestazione = async (prestazioneId: string): Promise<Slot[]> => {
  const response = await apiClient.get<Slot[]>(`/slot/prestazione/${prestazioneId}`);
  return response.data;
};

export const getPrenotazioniMie = async (): Promise<Prenotazione[]> => {
  const response = await apiClient.get<Prenotazione[]>("/prenotazioni/mie");
  return response.data;
};

export const getPrenotazioniAgenda = async (): Promise<Prenotazione[]> => {
  const response = await apiClient.get<Prenotazione[]>("/prenotazioni/agenda");
  return response.data;
};

export const getPrenotazioniTutte = async (): Promise<Prenotazione[]> => {
  const response = await apiClient.get<Prenotazione[]>("/prenotazioni");
  return response.data;
};

export const createPrenotazione = async (data: PrenotazioneInput): Promise<Prenotazione> => {
  const response = await apiClient.post<Prenotazione>("/prenotazioni", data);
  return response.data;
};

export const annullaPrenotazione = async (id: string): Promise<void> => {
  await apiClient.put(`/prenotazioni/${id}/annulla`);
};

// Conferma di presenza del paziente in risposta al promemoria.
export const confermaPresenza = async (id: string): Promise<void> => {
  await apiClient.put(`/prenotazioni/${id}/conferma-presenza`);
};

// Erogazione della visita da parte del medico (o admin): porta la prenotazione a Erogata.
export const erogaPrenotazione = async (id: string): Promise<void> => {
  await apiClient.put(`/prenotazioni/${id}/eroga`);
};

// Completamento amministrativo: genera la fattura e porta la prenotazione a Completata.
export const completaPrenotazione = async (id: string): Promise<void> => {
  await apiClient.put(`/prenotazioni/${id}/completa`);
};
