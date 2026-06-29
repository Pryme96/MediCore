import apiClient from "./client";
import type { Notifica } from "../types/notifiche";

export const getNotificheMie = async (): Promise<Notifica[]> => {
  const response = await apiClient.get<Notifica[]>("/notifiche/mie");
  return response.data;
};

export const getConteggioNonLette = async (): Promise<number> => {
  const response = await apiClient.get<number>("/notifiche/non-lette/count");
  return response.data;
};

export const marcaNotificaLetta = async (id: string): Promise<void> => {
  await apiClient.put(`/notifiche/${id}/letta`);
};
