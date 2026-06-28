import apiClient from "./client";
import type {
  Prestazione,
  PrestazioneInput,
  Servizio,
  ServizioInput,
  Tariffa,
  TariffaInput,
} from "../types/servizi";

export const getServizi = async (): Promise<Servizio[]> => {
  const response = await apiClient.get<Servizio[]>("/servizi");
  return response.data;
};

export const getPrestazioniPerServizio = async (servizioId: string): Promise<Prestazione[]> => {
  const response = await apiClient.get<Prestazione[]>(`/servizi/${servizioId}/prestazioni`);
  return response.data;
};

export const getPrestazioni = async (): Promise<Prestazione[]> => {
  const response = await apiClient.get<Prestazione[]>("/prestazioni");
  return response.data;
};

export const createServizio = async (data: ServizioInput): Promise<Servizio> => {
  const response = await apiClient.post<Servizio>("/servizi", data);
  return response.data;
};

export const updateServizio = async (id: string, data: ServizioInput): Promise<Servizio> => {
  const response = await apiClient.put<Servizio>(`/servizi/${id}`, data);
  return response.data;
};

export const createPrestazione = async (data: PrestazioneInput): Promise<Prestazione> => {
  const response = await apiClient.post<Prestazione>("/prestazioni", data);
  return response.data;
};

export const updatePrestazione = async (
  id: string,
  data: PrestazioneInput
): Promise<Prestazione> => {
  const response = await apiClient.put<Prestazione>(`/prestazioni/${id}`, data);
  return response.data;
};

export const getTariffePerPrestazione = async (prestazioneId: string): Promise<Tariffa[]> => {
  const response = await apiClient.get<Tariffa[]>(`/tariffe/prestazione/${prestazioneId}`);
  return response.data;
};

export const createTariffa = async (data: TariffaInput): Promise<Tariffa> => {
  const response = await apiClient.post<Tariffa>("/tariffe", data);
  return response.data;
};

export const updateTariffa = async (id: string, data: TariffaInput): Promise<Tariffa> => {
  const response = await apiClient.put<Tariffa>(`/tariffe/${id}`, data);
  return response.data;
};

export const deleteTariffa = async (id: string): Promise<void> => {
  await apiClient.delete(`/tariffe/${id}`);
};
