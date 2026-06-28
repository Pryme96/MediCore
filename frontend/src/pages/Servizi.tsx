import { useEffect, useState } from "react";
import { Alert, Collapse, List, Spin, Typography } from "antd";
import { getErrorMessage } from "../api/client";
import { getPrestazioniPerServizio, getServizi } from "../api/servizi";
import type { Prestazione, Servizio } from "../types/servizi";

export function Servizi() {
  const [servizi, setServizi] = useState<Servizio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [prestazioniPerServizio, setPrestazioniPerServizio] = useState<
    Record<string, Prestazione[]>
  >({});
  const [prestazioniLoading, setPrestazioniLoading] = useState<Record<string, boolean>>({});

  useEffect(() => {
    getServizi()
      .then(setServizi)
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare i servizi.")))
      .finally(() => setLoading(false));
  }, []);

  const handlePannelloAperto = (servizioId: string) => {
    if (prestazioniPerServizio[servizioId] || prestazioniLoading[servizioId]) {
      return;
    }
    setPrestazioniLoading((prev) => ({ ...prev, [servizioId]: true }));
    getPrestazioniPerServizio(servizioId)
      .then((prestazioni) =>
        setPrestazioniPerServizio((prev) => ({ ...prev, [servizioId]: prestazioni }))
      )
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare le prestazioni.")))
      .finally(() =>
        setPrestazioniLoading((prev) => ({ ...prev, [servizioId]: false }))
      );
  };

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2}>Servizi</Typography.Title>
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      <Collapse
        onChange={(keys) => {
          const aperti = Array.isArray(keys) ? keys : [keys];
          aperti.forEach((key) => handlePannelloAperto(key));
        }}
        items={servizi.map((servizio) => ({
          key: servizio.id,
          label: servizio.nome,
          children: prestazioniLoading[servizio.id] ? (
            <Spin size="small" />
          ) : (
            <List
              dataSource={prestazioniPerServizio[servizio.id] ?? []}
              locale={{ emptyText: "Nessuna prestazione disponibile per questo servizio." }}
              renderItem={(prestazione) => (
                <List.Item>
                  <List.Item.Meta
                    title={prestazione.nome}
                    description={`${prestazione.descrizione} — ${prestazione.durataMinuti} min`}
                  />
                </List.Item>
              )}
            />
          ),
        }))}
      />
    </div>
  );
}
