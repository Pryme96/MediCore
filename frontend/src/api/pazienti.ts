import apiClient from "./client";
import type { Paziente } from "../types/pazienti";

export const getPazienti = async (): Promise<Paziente[]> => {
  const response = await apiClient.get<Paziente[]>("/pazienti");
  return response.data;
};
