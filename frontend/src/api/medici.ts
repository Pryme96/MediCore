import apiClient from "./client";
import type {
  Medico,
  MedicoCreato,
  MedicoInput,
  MedicoUpdateInput,
  PasswordReset,
} from "../types/medici";

export const getMedici = async (): Promise<Medico[]> => {
  const response = await apiClient.get<Medico[]>("/medici");
  return response.data;
};

export const createMedico = async (data: MedicoInput): Promise<MedicoCreato> => {
  const response = await apiClient.post<MedicoCreato>("/medici", data);
  return response.data;
};

export const updateMedico = async (id: string, data: MedicoUpdateInput): Promise<void> => {
  await apiClient.put(`/medici/${id}`, data);
};

export const resetPasswordMedico = async (id: string): Promise<PasswordReset> => {
  const response = await apiClient.post<PasswordReset>(`/medici/${id}/reset-password`);
  return response.data;
};
