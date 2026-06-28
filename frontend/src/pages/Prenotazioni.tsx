import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Popconfirm, Space, Spin, Table, Tag, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import {
  annullaPrenotazione,
  getPrenotazioniAgenda,
  getPrenotazioniMie,
} from "../api/prenotazioni";
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

const colonnaDataOra = {
  title: "Data e ora",
  key: "dataora",
  render: (_: unknown, p: Prenotazione) =>
    `${dayjs(p.dataOraInizio).format("DD/MM/YYYY HH:mm")} – ${dayjs(p.dataOraFine).format("HH:mm")}`,
};

const colonnaRegime = {
  title: "Regime",
  dataIndex: "regime",
  render: (regime: Prenotazione["regime"]) => ETICHETTE_REGIME[regime],
};

const colonnaStato = {
  title: "Stato",
  dataIndex: "stato",
  render: (stato: StatoPrenotazione) => (
    <Tag color={COLORE_STATO[stato]}>{ETICHETTE_STATO_PRENOTAZIONE[stato]}</Tag>
  ),
};

export function Prenotazioni() {
  const { user } = useAuth();
  if (user?.ruoli.includes("Paziente")) {
    return <PrenotazioniPaziente />;
  }
  return <PrenotazioniOperatore isMedico={user?.ruoli.includes("Medico") ?? false} />;
}

// Vista Paziente: le proprie prenotazioni + stepper per prenotare per sé.
function PrenotazioniPaziente() {
  const [prenotazioni, setPrenotazioni] = useState<Prenotazione[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [modalita, setModalita] = useState<"lista" | "nuova">("lista");

  const carica = useCallback(async () => {
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
    carica();
  }, [carica]);

  const handleAnnulla = async (p: Prenotazione) => {
    setErrore("");
    try {
      await annullaPrenotazione(p.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile annullare la prenotazione."));
    }
  };

  if (modalita === "nuova") {
    return (
      <StepperPrenotazione
        onCompletato={() => {
          setModalita("lista");
          carica();
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
          colonnaDataOra,
          colonnaRegime,
          colonnaStato,
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

// Vista operatore (Amministratore/Medico): prenota per un paziente.
// Il Medico vede anche l'agenda delle prenotazioni sui propri turni, con possibilità di annullarle.
function PrenotazioniOperatore({ isMedico }: { isMedico: boolean }) {
  const [agenda, setAgenda] = useState<Prenotazione[]>([]);
  const [loading, setLoading] = useState(isMedico);
  const [errore, setErrore] = useState("");
  const [modalita, setModalita] = useState<"lista" | "nuova">("lista");

  const caricaAgenda = useCallback(async () => {
    if (!isMedico) {
      return;
    }
    setLoading(true);
    try {
      setAgenda(await getPrenotazioniAgenda());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare l'agenda."));
    } finally {
      setLoading(false);
    }
  }, [isMedico]);

  useEffect(() => {
    caricaAgenda();
  }, [caricaAgenda]);

  const handleAnnulla = async (p: Prenotazione) => {
    setErrore("");
    try {
      await annullaPrenotazione(p.id);
      caricaAgenda();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile annullare la prenotazione."));
    }
  };

  if (modalita === "nuova") {
    return (
      <StepperPrenotazione
        modalitaOperatore
        onCompletato={() => {
          setModalita("lista");
          caricaAgenda();
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
          {isMedico ? "Agenda prenotazioni" : "Prenotazioni"}
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalita("nuova")}>
          Prenota per un paziente
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      {isMedico ? (
        <Table
          dataSource={agenda}
          rowKey="id"
          pagination={false}
          locale={{ emptyText: "Nessuna prenotazione sui tuoi turni." }}
          columns={[
            { title: "Paziente", dataIndex: "pazienteNomeCompleto" },
            { title: "Prestazione", dataIndex: "prestazioneNome" },
            colonnaDataOra,
            colonnaRegime,
            colonnaStato,
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
      ) : (
        <Typography.Paragraph type="secondary">
          Usa "Prenota per un paziente" per registrare una prenotazione a nome di un paziente.
        </Typography.Paragraph>
      )}
    </div>
  );
}
