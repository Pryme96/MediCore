import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Space, Spin, Table, Tag, Typography } from "antd";
import { DownloadOutlined, UploadOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import { getPrenotazioniAgenda, getPrenotazioniMie } from "../api/prenotazioni";
import { downloadReferto, getRefertoPerPrenotazione } from "../api/referti";
import { StatoPrenotazione, type Prenotazione } from "../types/prenotazioni";
import type { Referto } from "../types/referti";
import { UploadRefertoModal } from "../components/referti/UploadRefertoModal";

// Solo le prenotazioni per cui ha senso un referto: visite confermate o già completate.
const STATI_REFERTABILI: number[] = [StatoPrenotazione.Confermata, StatoPrenotazione.Completata];

const nomeFileReferto = (p: Prenotazione) =>
  `referto-${p.prestazioneNome}-${dayjs(p.dataOraInizio).format("YYYYMMDD")}.pdf`
    .replace(/\s+/g, "_");

export function Referti() {
  const { user } = useAuth();
  if (user?.ruoli.includes("Medico")) {
    return <RefertiMedico />;
  }
  return <RefertiPaziente />;
}

// Carica le prenotazioni refertabili e, per ognuna, l'eventuale referto associato.
function useRefertiPrenotazioni(carica: () => Promise<Prenotazione[]>) {
  const [prenotazioni, setPrenotazioni] = useState<Prenotazione[]>([]);
  const [referti, setReferti] = useState<Record<string, Referto | null>>({});
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  const ricarica = useCallback(async () => {
    setLoading(true);
    setErrore("");
    try {
      const lista = (await carica()).filter((p) => STATI_REFERTABILI.includes(p.stato));
      const coppie = await Promise.all(
        lista.map(async (p) => [p.id, await getRefertoPerPrenotazione(p.id)] as const)
      );
      setPrenotazioni(lista);
      setReferti(Object.fromEntries(coppie));
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare i referti."));
    } finally {
      setLoading(false);
    }
  }, [carica]);

  useEffect(() => {
    ricarica();
  }, [ricarica]);

  return { prenotazioni, referti, loading, errore, ricarica };
}

function TagReferto({ referto }: { referto: Referto | null | undefined }) {
  return referto ? (
    <Tag color="green">Disponibile</Tag>
  ) : (
    <Tag>Non disponibile</Tag>
  );
}

function PulsanteScarica({ prenotazione, referto }: { prenotazione: Prenotazione; referto: Referto | null | undefined }) {
  const [scaricando, setScaricando] = useState(false);
  if (!referto) {
    return null;
  }
  const scarica = async () => {
    setScaricando(true);
    try {
      await downloadReferto(referto.id, nomeFileReferto(prenotazione));
    } finally {
      setScaricando(false);
    }
  };
  return (
    <Button size="small" icon={<DownloadOutlined />} loading={scaricando} onClick={scarica}>
      Scarica
    </Button>
  );
}

// Vista Medico: agenda con caricamento/sostituzione del referto.
function RefertiMedico() {
  const { prenotazioni, referti, loading, errore, ricarica } = useRefertiPrenotazioni(getPrenotazioniAgenda);
  const [selezionata, setSelezionata] = useState<Prenotazione | null>(null);

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2} style={{ marginTop: 0, marginBottom: 16 }}>
        Referti
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={prenotazioni}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Nessuna visita per cui caricare un referto." }}
        columns={[
          { title: "Paziente", dataIndex: "pazienteNomeCompleto" },
          { title: "Prestazione", dataIndex: "prestazioneNome" },
          {
            title: "Data",
            dataIndex: "dataOraInizio",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY HH:mm"),
          },
          {
            title: "Referto",
            key: "referto",
            render: (_, p) => <TagReferto referto={referti[p.id]} />,
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) => (
              <Space>
                <Button
                  size="small"
                  icon={<UploadOutlined />}
                  onClick={() => setSelezionata(p)}
                >
                  {referti[p.id] ? "Sostituisci" : "Carica"}
                </Button>
                <PulsanteScarica prenotazione={p} referto={referti[p.id]} />
              </Space>
            ),
          },
        ]}
      />

      <UploadRefertoModal
        open={selezionata !== null}
        prenotazione={selezionata}
        refertoEsistente={selezionata ? Boolean(referti[selezionata.id]) : false}
        onClose={() => setSelezionata(null)}
        onCaricato={() => {
          setSelezionata(null);
          ricarica();
        }}
      />
    </div>
  );
}

// Vista Paziente: sola lettura/download dei propri referti.
function RefertiPaziente() {
  const { prenotazioni, referti, loading, errore } = useRefertiPrenotazioni(getPrenotazioniMie);

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2} style={{ marginTop: 0, marginBottom: 16 }}>
        I miei referti
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={prenotazioni}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Non hai ancora referti disponibili." }}
        columns={[
          { title: "Medico", dataIndex: "medicoNomeCompleto" },
          { title: "Prestazione", dataIndex: "prestazioneNome" },
          {
            title: "Data",
            dataIndex: "dataOraInizio",
            render: (data: string) => dayjs(data).format("DD/MM/YYYY HH:mm"),
          },
          {
            title: "Referto",
            key: "referto",
            render: (_, p) => <TagReferto referto={referti[p.id]} />,
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) => <PulsanteScarica prenotazione={p} referto={referti[p.id]} />,
          },
        ]}
      />
    </div>
  );
}
