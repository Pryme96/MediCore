import apiClient from "./client";
import type { Prescrizione, PrescrizioneInput } from "../types/prescrizioni";

export const getPrescrizioniMie = async (): Promise<Prescrizione[]> => {
  const response = await apiClient.get<Prescrizione[]>("/prescrizioni/mie");
  return response.data;
};

export const getPrescrizioniEmesse = async (): Promise<Prescrizione[]> => {
  const response = await apiClient.get<Prescrizione[]>("/prescrizioni/emesse");
  return response.data;
};

export const getPrescrizioneById = async (id: string): Promise<Prescrizione> => {
  const response = await apiClient.get<Prescrizione>(`/prescrizioni/${id}`);
  return response.data;
};

export const createPrescrizione = async (data: PrescrizioneInput): Promise<Prescrizione> => {
  const response = await apiClient.post<Prescrizione>("/prescrizioni", data);
  return response.data;
};
