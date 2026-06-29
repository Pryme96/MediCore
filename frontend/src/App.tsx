import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { AppLayout } from "./layout/AppLayout";
import { Login } from "./pages/Login";
import { Register } from "./pages/Register";
import { Home } from "./pages/Home";
import { Servizi } from "./pages/Servizi";
import { GestioneServizi } from "./pages/GestioneServizi";
import { Medici } from "./pages/Medici";
import { Turni } from "./pages/Turni";
import { Prenotazioni } from "./pages/Prenotazioni";
import { Prescrizioni } from "./pages/Prescrizioni";
import { Referti } from "./pages/Referti";
import { Fatture } from "./pages/Fatture";
import { Notifiche } from "./pages/Notifiche";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Home />} />
            <Route path="servizi" element={<Servizi />} />
            <Route
              path="prenotazioni"
              element={
                <ProtectedRoute ruoliConsentiti={["Paziente", "Amministratore", "Medico"]}>
                  <Prenotazioni />
                </ProtectedRoute>
              }
            />
            <Route
              path="prescrizioni"
              element={
                <ProtectedRoute ruoliConsentiti={["Paziente", "Medico"]}>
                  <Prescrizioni />
                </ProtectedRoute>
              }
            />
            <Route
              path="referti"
              element={
                <ProtectedRoute ruoliConsentiti={["Paziente", "Medico"]}>
                  <Referti />
                </ProtectedRoute>
              }
            />
            <Route
              path="fatture"
              element={
                <ProtectedRoute ruoliConsentiti={["Paziente", "Amministratore"]}>
                  <Fatture />
                </ProtectedRoute>
              }
            />
            <Route
              path="notifiche"
              element={
                <ProtectedRoute ruoliConsentiti={["Paziente"]}>
                  <Notifiche />
                </ProtectedRoute>
              }
            />
            <Route
              path="medici"
              element={
                <ProtectedRoute ruoliConsentiti={["Amministratore"]}>
                  <Medici />
                </ProtectedRoute>
              }
            />
            <Route
              path="turni"
              element={
                <ProtectedRoute ruoliConsentiti={["Amministratore", "Medico"]}>
                  <Turni />
                </ProtectedRoute>
              }
            />
            <Route
              path="gestione-servizi"
              element={
                <ProtectedRoute ruoliConsentiti={["Amministratore"]}>
                  <GestioneServizi />
                </ProtectedRoute>
              }
            />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
