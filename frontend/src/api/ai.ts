import apiClient from "./client";
import type { SuggerimentoRequest, SuggerimentoResponse } from "../types/ai";

export const suggerisciPrescrizione = async (data: SuggerimentoRequest): Promise<SuggerimentoResponse> => {
  const response = await apiClient.post<SuggerimentoResponse>("/ai/suggerisci", data);
  return response.data;
};
