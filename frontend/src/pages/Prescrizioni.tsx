import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Space, Spin, Table, Tag, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import { getPrescrizioniEmesse, getPrescrizioniMie } from "../api/prescrizioni";
import {
  ETICHETTE_TIPO_PRESCRIZIONE,
  TipoPrescrizione,
  type Prescrizione,
} from "../types/prescrizioni";

const TIPO_COLUMN = {
  title: "Tipo",
  dataIndex: "tipo",
  render: (tipo: TipoPrescrizione) => (
    <Tag color={tipo === TipoPrescrizione.PianoTerapeutico ? "purple" : "blue"}>
      {ETICHETTE_TIPO_PRESCRIZIONE[tipo]}
    </Tag>
  ),
};
import { PrescrizioneFormModal } from "../components/prescrizioni/PrescrizioneFormModal";
import { DettaglioPrescrizione } from "../components/prescrizioni/DettaglioPrescrizione";

export function Prescrizioni() {
  const { user } = useAuth();
  if (user?.ruoli.includes("Medico")) {
    return <PrescrizioniMedico />;
  }
  return <PrescrizioniPaziente />;
}

// Vista Medico: le prescrizioni emesse + creazione di una nuova prescrizione.
function PrescrizioniMedico() {
  const [prescrizioni, setPrescrizioni] = useState<Prescrizione[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [formAperto, setFormAperto] = useState(false);
  const [dettaglio, setDettaglio] = useState<Prescrizione | null>(null);

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setPrescrizioni(await getPrescrizioniEmesse());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le prescrizioni."));
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
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          Prescrizioni emesse
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setFormAperto(true)}>
          Nuova prescrizione
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={prescrizioni}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Non hai ancora emesso prescrizioni." }}
        columns={[
          { title: "Paziente", dataIndex: "pazienteNomeCompleto" },
          TIPO_COLUMN,
          {
            title: "Emessa il",
            dataIndex: "dataEmissione",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY"),
          },
          {
            title: "Scade il",
            dataIndex: "dataScadenza",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY"),
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) => (
              <Button size="small" onClick={() => setDettaglio(p)}>
                Dettaglio
              </Button>
            ),
          },
        ]}
      />

      <PrescrizioneFormModal
        open={formAperto}
        onClose={() => setFormAperto(false)}
        onCreata={() => {
          setFormAperto(false);
          carica();
        }}
      />
      <DettaglioPrescrizione prescrizione={dettaglio} onChiudi={() => setDettaglio(null)} />
    </div>
  );
}

// Vista Paziente: le proprie prescrizioni in sola lettura.
function PrescrizioniPaziente() {
  const [prescrizioni, setPrescrizioni] = useState<Prescrizione[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [dettaglio, setDettaglio] = useState<Prescrizione | null>(null);

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setPrescrizioni(await getPrescrizioniMie());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le prescrizioni."));
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
        Le mie prescrizioni
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={prescrizioni}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Non hai ancora prescrizioni." }}
        columns={[
          { title: "Medico", dataIndex: "medicoNomeCompleto" },
          TIPO_COLUMN,
          {
            title: "Emessa il",
            dataIndex: "dataEmissione",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY"),
          },
          {
            title: "Scade il",
            dataIndex: "dataScadenza",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY"),
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) => (
              <Button size="small" onClick={() => setDettaglio(p)}>
                Dettaglio
              </Button>
            ),
          },
        ]}
      />

      <DettaglioPrescrizione
        prescrizione={dettaglio}
        mostraMedico
        onChiudi={() => setDettaglio(null)}
      />
    </div>
  );
}
