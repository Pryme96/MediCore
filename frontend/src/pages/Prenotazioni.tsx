import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Popconfirm, Space, Spin, Table, Tag, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import {
  annullaPrenotazione,
  completaPrenotazione,
  confermaPresenza,
  erogaPrenotazione,
  getPrenotazioniAgenda,
  getPrenotazioniMie,
  getPrenotazioniTutte,
} from "../api/prenotazioni";
import {
  ETICHETTE_STATO_PRENOTAZIONE,
  StatoPrenotazione,
  type Prenotazione,
} from "../types/prenotazioni";
import { ETICHETTE_REGIME } from "../types/servizi";
import { StepperPrenotazione } from "../components/prenotazioni/StepperPrenotazione";
import { ElencoPrenotazioni } from "../components/prenotazioni/ElencoPrenotazioni";

const COLORE_STATO: Record<StatoPrenotazione, string> = {
  [StatoPrenotazione.Confermata]: "green",
  [StatoPrenotazione.Annullata]: "default",
  [StatoPrenotazione.Completata]: "blue",
  [StatoPrenotazione.NonPresentato]: "red",
  [StatoPrenotazione.Erogata]: "gold",
};

export function Prenotazioni() {
  const { user } = useAuth();
  if (user?.ruoli.includes("Amministratore")) {
    return <PrenotazioniOperatore variante="amministratore" />;
  }
  if (user?.ruoli.includes("Medico")) {
    return <PrenotazioniOperatore variante="medico" />;
  }
  return <PrenotazioniPaziente />;
}

// Vista Paziente: le proprie prenotazioni (lista semplice) + stepper per prenotare per sé.
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

  const handleConferma = async (p: Prenotazione) => {
    setErrore("");
    try {
      await confermaPresenza(p.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile confermare la presenza."));
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
            key: "stato",
            render: (_, p) => (
              <Space>
                <Tag color={COLORE_STATO[p.stato]}>{ETICHETTE_STATO_PRENOTAZIONE[p.stato]}</Tag>
                {p.confermataDalPaziente && <Tag color="green">Presenza confermata</Tag>}
              </Space>
            ),
          },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, p) =>
              p.stato === StatoPrenotazione.Confermata ? (
                <Space>
                  {!p.confermataDalPaziente && (
                    <Button size="small" type="primary" onClick={() => handleConferma(p)}>
                      Conferma presenza
                    </Button>
                  )}
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
                </Space>
              ) : null,
          },
        ]}
      />
    </div>
  );
}

// Vista operatore (Amministratore = tutte le prenotazioni; Medico = agenda dei propri turni).
// Entrambe con filtri e toggle tabella/calendario, più lo stepper per prenotare per un paziente.
function PrenotazioniOperatore({ variante }: { variante: "amministratore" | "medico" }) {
  const isAmministratore = variante === "amministratore";
  const [prenotazioni, setPrenotazioni] = useState<Prenotazione[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [modalita, setModalita] = useState<"lista" | "nuova">("lista");

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setPrenotazioni(isAmministratore ? await getPrenotazioniTutte() : await getPrenotazioniAgenda());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le prenotazioni."));
    } finally {
      setLoading(false);
    }
  }, [isAmministratore]);

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

  const handleEroga = async (p: Prenotazione) => {
    setErrore("");
    try {
      await erogaPrenotazione(p.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile segnare la visita come erogata."));
    }
  };

  const handleCompleta = async (p: Prenotazione) => {
    setErrore("");
    try {
      await completaPrenotazione(p.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile generare la fattura: verifica che sia configurata una tariffa per la prestazione e il regime."));
    }
  };

  if (modalita === "nuova") {
    return (
      <StepperPrenotazione
        modalitaOperatore
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
          {isAmministratore ? "Prenotazioni" : "Agenda prenotazioni"}
        </Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalita("nuova")}>
          Prenota per un paziente
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <ElencoPrenotazioni
        prenotazioni={prenotazioni}
        onAnnulla={handleAnnulla}
        onEroga={isAmministratore ? undefined : handleEroga}
        onCompleta={isAmministratore ? handleCompleta : undefined}
        mostraMedico={isAmministratore}
        emptyText={
          isAmministratore ? "Nessuna prenotazione corrisponde ai filtri." : "Nessuna prenotazione sui tuoi turni."
        }
      />
    </div>
  );
}
