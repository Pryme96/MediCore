export interface Medico {
  id: string;
  email: string;
  nome: string;
  cognome: string;
  specializzazione: string;
  servizioId: string;
  servizioNome: string;
}

export interface MedicoCreato {
  medico: Medico;
  passwordGenerata: string;
}

export interface PasswordReset {
  passwordGenerata: string;
}

export interface MedicoInput {
  email: string;
  nome: string;
  cognome: string;
  specializzazione: string;
  servizioId: string;
}

export interface MedicoUpdateInput {
  specializzazione: string;
  servizioId: string;
}
