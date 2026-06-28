import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Popconfirm, Space, Spin, Table, Tag, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { getErrorMessage } from "../api/client";
import { annullaPrenotazione, getPrenotazioniMie } from "../api/prenotazioni";
import {
  ETICHETTE_STATO_PRENOTAZIONE,
  StatoPrenotazione,
  type Prenotazione,
} from "../types/prenotazioni";
import { ETICHETTE_REGIME } from "../types/servizi";
import { StepperPrenotazione } from "../components/prenotazioni/StepperPrenotazione";

const COLORE_STATO: Record<StatoPrenotazione, string> = {
  [StatoPrenotazione.Confermata]: "green",
  [StatoPrenotazione.Annullata]: "default",
  [StatoPrenotazione.Completata]: "blue",
  [StatoPrenotazione.NonPresentato]: "red",
};

export function Prenotazioni() {
  const [prenotazioni, setPrenotazioni] = useState<Prenotazione[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [modalita, setModalita] = useState<"lista" | "nuova">("lista");

  const caricaPrenotazioni = useCallback(async () => {
    setLoading(true);
    try {
      setPrenotazioni(await getPrenotazioniMie());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le prenotazioni."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    caricaPrenotazioni();
  }, [caricaPrenotazioni]);

  const handleAnnulla = async (prenotazione: Prenotazione) => {
    setErrore("");
    try {
      await annullaPrenotazione(prenotazione.id);
      caricaPrenotazioni();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile annullare la prenotazione."));
    }
  };

  if (modalita === "nuova") {
    return (
      <StepperPrenotazione
        onCompletato={() => {
          setModalita("lista");
          caricaPrenotazioni();
        }}
        onAnnulla={() => setModalita("lista")}
      />
    );
  }

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          Le mie prenotazioni
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalita("nuova")}>
          Prenota una visita
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={prenotazioni}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Non hai ancora prenotazioni." }}
        columns={[
          { title: "Prestazione", dataIndex: "prestazioneNome" },
          { title: "Medico", dataIndex: "medicoNomeCompleto" },
          {
            title: "Data e ora",
            key: "dataora",
            render: (_, p) =>
              `${dayjs(p.dataOraInizio).format("DD/MM/YYYY HH:mm")} – ${dayjs(p.dataOraFine).format("HH:mm")}`,
          },
          {
            title: "Regime",
            dataIndex: "regime",
            render: (regime: Prenotazione["regime"]) => ETICHETTE_REGIME[regime],
          },
          {
            title: "Stato",
            dataIndex: "stato",
            render: (stato: StatoPrenotazione) => (
              <Tag color={COLORE_STATO[stato]}>{ETICHETTE_STATO_PRENOTAZIONE[stato]}</Tag>
            ),
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) =>
              p.stato === StatoPrenotazione.Confermata ? (
                <Popconfirm
                  title="Annullare la prenotazione?"
                  okText="Annulla prenotazione"
                  cancelText="No"
                  onConfirm={() => handleAnnulla(p)}
                >
                  <Button size="small" danger>
                    Annulla
                  </Button>
                </Popconfirm>
              ) : null,
          },
        ]}
      />
    </div>
  );
}
