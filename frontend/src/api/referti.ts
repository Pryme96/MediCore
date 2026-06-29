import { isAxiosError } from "axios";
import apiClient from "./client";
import type { Referto } from "../types/referti";

// Restituisce il referto associato alla prenotazione, oppure null se non esiste (404).
export const getRefertoPerPrenotazione = async (
  prenotazioneId: string
): Promise<Referto | null> => {
  try {
    const response = await apiClient.get<Referto>(`/referti/prenotazione/${prenotazioneId}`);
    return response.data;
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
};

export const uploadReferto = async (
  prenotazioneId: string,
  file: File,
  contenuto?: string
): Promise<Referto> => {
  const formData = new FormData();
  formData.append("PrenotazioneId", prenotazioneId);
  formData.append("File", file);
  if (contenuto) {
    formData.append("Contenuto", contenuto);
  }
  const response = await apiClient.post<Referto>("/referti", formData);
  return response.data;
};

// Scarica il PDF del referto e ne avvia il download nel browser.
export const downloadReferto = async (refertoId: string, nomeFile: string): Promise<void> => {
  const response = await apiClient.get(`/referti/${refertoId}/file`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data);
  const link = document.createElement("a");
  link.href = url;
  link.download = nomeFile;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
};
