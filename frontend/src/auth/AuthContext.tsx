import { createContext, useContext, useState, type ReactNode } from "react";

interface AuthUser {
  email: string;
  nome: string;
  cognome: string;
  ruoli: string[];
}

interface AuthContextValue {
  user: AuthUser | null;
  loginWithToken: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function decodeUserFromToken(token: string): AuthUser {
  const payload = JSON.parse(atob(token.split(".")[1]));
  const rolesClaim =
    payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? payload.role ?? [];
  const emailClaim =
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ?? payload.email ?? "";
  const nomeClaim =
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"] ?? payload.given_name ?? "";
  const cognomeClaim =
    payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"] ?? payload.family_name ?? "";

  return {
    email: emailClaim,
    nome: nomeClaim,
    cognome: cognomeClaim,
    ruoli: Array.isArray(rolesClaim) ? rolesClaim : [rolesClaim],
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const token = localStorage.getItem("token");
    return token ? decodeUserFromToken(token) : null;
  });

  const loginWithToken = (token: string) => {
    localStorage.setItem("token", token);
    setUser(decodeUserFromToken(token));
  };

  const logout = () => {
    localStorage.removeItem("token");
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loginWithToken, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth deve essere usato dentro AuthProvider");
  }
  return context;
}
