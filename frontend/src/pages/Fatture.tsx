import { useCallback, useEffect, useMemo, useState } from "react";
import { Alert, Input, Space, Spin, Table, Tag, Typography } from "antd";
import dayjs from "dayjs";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import { getFattureMie, getFattureTutte } from "../api/fatture";
import { ETICHETTE_STATO_FATTURA, StatoFattura, type Fattura } from "../types/fatture";
import { ETICHETTE_REGIME } from "../types/servizi";

const COLORE_STATO: Record<StatoFattura, string> = {
  [StatoFattura.Emessa]: "blue",
  [StatoFattura.Pagata]: "green",
  [StatoFattura.Scaduta]: "red",
  [StatoFattura.Annullata]: "default",
};

const formatImporto = (importo: number) =>
  new Intl.NumberFormat("it-IT", { style: "currency", currency: "EUR" }).format(importo);

const colonneComuni = [
  {
    title: "Emessa il",
    dataIndex: "dataEmissione",
    render: (data: string) => dayjs(data).format("DD/MM/YYYY"),
  },
  {
    title: "Importo",
    dataIndex: "importo",
    align: "right" as const,
    render: (importo: number) => formatImporto(importo),
  },
  {
    title: "Regime",
    dataIndex: "regime",
    render: (regime: Fattura["regime"]) => ETICHETTE_REGIME[regime],
  },
  {
    title: "Stato",
    dataIndex: "stato",
    render: (stato: StatoFattura) => (
      <Tag color={COLORE_STATO[stato]}>{ETICHETTE_STATO_FATTURA[stato]}</Tag>
    ),
  },
];

export function Fatture() {
  const { user } = useAuth();
  if (user?.ruoli.includes("Amministratore")) {
    return <FattureAmministratore />;
  }
  return <FatturePaziente />;
}

// Vista Amministratore: tutte le fatture, con ricerca per paziente.
function FattureAmministratore() {
  const [fatture, setFatture] = useState<Fattura[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [ricerca, setRicerca] = useState("");

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setFatture(await getFattureTutte());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le fatture."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carica();
  }, [carica]);

  const fattureFiltrate = useMemo(() => {
    const testo = ricerca.trim().toLowerCase();
    if (!testo) return fatture;
    return fatture.filter((f) => f.pazienteNomeCompleto.toLowerCase().includes(testo));
  }, [fatture, ricerca]);

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2} style={{ marginTop: 0, marginBottom: 16 }}>
        Fatture
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Space style={{ marginBottom: 12 }}>
        <Input.Search
          allowClear
          placeholder="Cerca paziente"
          style={{ width: 280 }}
          value={ricerca}
          onChange={(e) => setRicerca(e.target.value)}
        />
      </Space>

      <Table
        dataSource={fattureFiltrate}
        rowKey="id"
        pagination={{ pageSize: 10, showSizeChanger: false }}
        locale={{ emptyText: "Nessuna fattura emessa." }}
        columns={[{ title: "Paziente", dataIndex: "pazienteNomeCompleto" }, ...colonneComuni]}
      />
    </div>
  );
}

// Vista Paziente: le proprie fatture in sola lettura.
function FatturePaziente() {
  const [fatture, setFatture] = useState<Fattura[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setFatture(await getFattureMie());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le fatture."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carica();
  }, [carica]);

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2} style={{ marginTop: 0, marginBottom: 16 }}>
        Le mie fatture
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={fatture}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Non hai ancora fatture." }}
        columns={colonneComuni}
      />
    </div>
  );
}
