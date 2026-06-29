import { useEffect, useState } from "react";
import { Badge, Button } from "antd";
import { BellOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { getConteggioNonLette } from "../../api/notifiche";

// Campanello nell'header: mostra il numero di notifiche non lette e porta al centro notifiche.
// Il conteggio viene ricaricato a ogni cambio di pagina (così si aggiorna dopo la lettura).
export function CampanelloNotifiche() {
  const [nonLette, setNonLette] = useState(0);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    let attivo = true;
    getConteggioNonLette()
      .then((conteggio) => {
        if (attivo) setNonLette(conteggio);
      })
      .catch(() => {
        // Il badge è un di più: in caso di errore non si disturba l'utente.
      });
    return () => {
      attivo = false;
    };
  }, [location]);

  return (
    <Badge count={nonLette} size="small">
      <Button
        type="text"
        shape="circle"
        icon={<BellOutlined style={{ fontSize: 18 }} />}
        aria-label="Notifiche"
        onClick={() => navigate("/notifiche")}
      />
    </Badge>
  );
}
