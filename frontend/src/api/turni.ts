import apiClient from "./client";
import type { Turno, TurnoInput } from "../types/turni";

export const getTurni = async (): Promise<Turno[]> => {
  const response = await apiClient.get<Turno[]>("/turni");
  return response.data;
};

export const getTurniPerMedico = async (medicoId: string): Promise<Turno[]> => {
  const response = await apiClient.get<Turno[]>(`/turni/medico/${medicoId}`);
  return response.data;
};

export const getTurniMiei = async (): Promise<Turno[]> => {
  const response = await apiClient.get<Turno[]>("/turni/miei");
  return response.data;
};

export const createTurno = async (data: TurnoInput): Promise<Turno> => {
  const response = await apiClient.post<Turno>("/turni", data);
  return response.data;
};

export const updateTurno = async (id: string, data: TurnoInput): Promise<void> => {
  await apiClient.put(`/turni/${id}`, data);
};

export const deleteTurno = async (id: string): Promise<void> => {
  await apiClient.delete(`/turni/${id}`);
};
