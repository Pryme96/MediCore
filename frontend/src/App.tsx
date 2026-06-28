import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { AppLayout } from "./layout/AppLayout";
import { Login } from "./pages/Login";
import { Register } from "./pages/Register";
import { Home } from "./pages/Home";
import { PlaceholderPage } from "./pages/PlaceholderPage";
import { Servizi } from "./pages/Servizi";
import { GestioneServizi } from "./pages/GestioneServizi";

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
            <Route path="prenotazioni" element={<PlaceholderPage titolo="Prenotazioni" />} />
            <Route path="prescrizioni" element={<PlaceholderPage titolo="Prescrizioni" />} />
            <Route path="referti" element={<PlaceholderPage titolo="Referti" />} />
            <Route path="fatture" element={<PlaceholderPage titolo="Fatture" />} />
            <Route path="medici" element={<PlaceholderPage titolo="Medici" />} />
            <Route path="turni" element={<PlaceholderPage titolo="Turni" />} />
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
