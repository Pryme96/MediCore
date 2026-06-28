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

export const createPrenotazione = async (data: PrenotazioneInput): Promise<Prenotazione> => {
  const response = await apiClient.post<Prenotazione>("/prenotazioni", data);
  return response.data;
};

export const annullaPrenotazione = async (id: string): Promise<void> => {
  await apiClient.put(`/prenotazioni/${id}/annulla`);
};
