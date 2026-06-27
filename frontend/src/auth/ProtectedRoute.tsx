import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

interface ProtectedRouteProps {
  children: ReactNode;
  ruoliConsentiti?: string[];
}

export function ProtectedRoute({ children, ruoliConsentiti }: ProtectedRouteProps) {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (ruoliConsentiti && !ruoliConsentiti.some((ruolo) => user.ruoli.includes(ruolo))) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
