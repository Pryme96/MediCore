import apiClient from "./client";
import type { Fattura } from "../types/fatture";

export const getFattureMie = async (): Promise<Fattura[]> => {
  const response = await apiClient.get<Fattura[]>("/fatture/mie");
  return response.data;
};

export const getFattureTutte = async (): Promise<Fattura[]> => {
  const response = await apiClient.get<Fattura[]>("/fatture");
  return response.data;
};
