export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nome: string;
  cognome: string;
  codiceFiscale: string;
  dataNascita: string;
  telefono: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
  ruoli: string[];
}
