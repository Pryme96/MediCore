import axios, { isAxiosError } from "axios";

export function getErrorMessage(error: unknown, messaggioRichiesta: string): string {
  if (isAxiosError(error)) {
    if (!error.response) {
      return "Impossibile contattare il server. Verifica la connessione e riprova.";
    }
    return messaggioRichiesta;
  }
  return "Si è verificato un errore imprevisto. Riprova.";
}

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const url = error.config?.url ?? "";
    const isAuthRequest = url.includes("/auth/login") || url.includes("/auth/register");
    if (error.response?.status === 401 && !isAuthRequest) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default apiClient;
