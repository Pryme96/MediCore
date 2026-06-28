import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Popconfirm, Segmented, Select, Space, Spin, Table, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import { deleteTurno, getTurni, getTurniMiei, getTurniPerMedico } from "../api/turni";
import { getMedici } from "../api/medici";
import { getPrestazioni } from "../api/servizi";
import { ETICHETTE_GIORNO, type Turno } from "../types/turni";
import type { Medico } from "../types/medici";
import type { Prestazione } from "../types/servizi";
import { TurnoFormModal } from "../components/turni/TurnoFormModal";
import { CalendarioSettimanale } from "../components/turni/CalendarioSettimanale";
import { GrigliaMediciSettimanale } from "../components/turni/GrigliaMediciSettimanale";

const oraBreve = (ora: string) => ora.slice(0, 5);

type VistaTurni = "tabella" | "calendario";

const OPZIONI_VISTA = [
  { label: "Tabella", value: "tabella" },
  { label: "Calendario", value: "calendario" },
];

export function Turni() {
  const { user } = useAuth();
  const isAmministratore = user?.ruoli.includes("Amministratore") ?? false;
  return isAmministratore ? <TurniAmministratore /> : <TurniMedico />;
}

// Vista di gestione completa (solo Amministratore): tabella con creazione/modifica/eliminazione.
function TurniAmministratore() {
  const [turni, setTurni] = useState<Turno[]>([]);
  const [medici, setMedici] = useState<Medico[]>([]);
  const [prestazioni, setPrestazioni] = useState<Prestazione[]>([]);
  const [filtroMedicoId, setFiltroMedicoId] = useState<string | null>(null);
  const [vista, setVista] = useState<VistaTurni>("tabella");
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  const [formModal, setFormModal] = useState<{ open: boolean; turno: Turno | null }>({
    open: false,
    turno: null,
  });

  const caricaTurni = useCallback(async (medicoId: string | null) => {
    try {
      setTurni(medicoId ? await getTurniPerMedico(medicoId) : await getTurni());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare i turni."));
    }
  }, []);

  const caricaDati = useCallback(async () => {
    setLoading(true);
    try {
      const [listaTurni, listaMedici, listaPrestazioni] = await Promise.all([
        getTurni(),
        getMedici(),
        getPrestazioni(),
      ]);
      setTurni(listaTurni);
      setMedici(listaMedici);
      setPrestazioni(listaPrestazioni);
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare i turni."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    caricaDati();
  }, [caricaDati]);

  const handleFiltroMedico = (medicoId: string | null) => {
    setFiltroMedicoId(medicoId);
    caricaTurni(medicoId);
  };

  const handleSaved = () => {
    setFormModal({ open: false, turno: null });
    caricaTurni(filtroMedicoId);
  };

  const handleElimina = async (turno: Turno) => {
    setErrore("");
    try {
      await deleteTurno(turno.id);
      caricaTurni(filtroMedicoId);
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile eliminare il turno."));
    }
  };

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          Turni
        </Typography.Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setFormModal({ open: true, turno: null })}
        >
          Nuovo turno
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Space>
          <Typography.Text>Filtra per medico:</Typography.Text>
          <Select
            allowClear
            placeholder="Tutti i medici"
            style={{ minWidth: 260 }}
            value={filtroMedicoId}
            onChange={(value) => handleFiltroMedico(value ?? null)}
            options={medici.map((m) => ({
              value: m.id,
              label: `${m.cognome} ${m.nome} — ${m.specializzazione}`,
            }))}
          />
        </Space>
        <Segmented
          options={OPZIONI_VISTA}
          value={vista}
          onChange={(value) => setVista(value as VistaTurni)}
        />
      </Space>

      {vista === "tabella" ? (
        <Table
          dataSource={turni}
          rowKey="id"
          pagination={false}
          locale={{ emptyText: "Nessun turno configurato." }}
          columns={[
            { title: "Medico", dataIndex: "medicoNomeCompleto" },
            { title: "Prestazione", dataIndex: "prestazioneNome" },
            {
              title: "Giorno",
              dataIndex: "giornoSettimana",
              render: (giorno: Turno["giornoSettimana"]) => ETICHETTE_GIORNO[giorno],
            },
            {
              title: "Orario",
              key: "orario",
              render: (_, turno) => `${oraBreve(turno.oraInizio)} – ${oraBreve(turno.oraFine)}`,
            },
            { title: "Durata slot (min)", dataIndex: "durataSlotMin" },
            {
              title: "Azioni",
              key: "azioni",
              render: (_, turno) => (
                <Space>
                  <Button size="small" onClick={() => setFormModal({ open: true, turno })}>
                    Modifica
                  </Button>
                  <Popconfirm
                    title="Eliminare il turno?"
                    okText="Elimina"
                    cancelText="Annulla"
                    onConfirm={() => handleElimina(turno)}
                  >
                    <Button size="small" danger>
                      Elimina
                    </Button>
                  </Popconfirm>
                </Space>
              ),
            },
          ]}
        />
      ) : (
        <GrigliaMediciSettimanale
          medici={filtroMedicoId ? medici.filter((m) => m.id === filtroMedicoId) : medici}
          turni={turni}
        />
      )}

      <TurnoFormModal
        open={formModal.open}
        turno={formModal.turno}
        medici={medici}
        prestazioni={prestazioni}
        onClose={() => setFormModal({ open: false, turno: null })}
        onSaved={handleSaved}
      />
    </div>
  );
}

// Vista in sola lettura per il Medico: solo i propri turni, nessuna azione di gestione.
function TurniMedico() {
  const [turni, setTurni] = useState<Turno[]>([]);
  const [vista, setVista] = useState<VistaTurni>("tabella");
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  useEffect(() => {
    getTurniMiei()
      .then(setTurni)
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare i turni.")))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          I miei turni
        </Typography.Title>
        <Segmented
          options={OPZIONI_VISTA}
          value={vista}
          onChange={(value) => setVista(value as VistaTurni)}
        />
      </Space>
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      {vista === "tabella" ? (
        <Table
          dataSource={turni}
          rowKey="id"
          pagination={false}
          locale={{ emptyText: "Non hai turni assegnati." }}
          columns={[
            { title: "Prestazione", dataIndex: "prestazioneNome" },
            {
              title: "Giorno",
              dataIndex: "giornoSettimana",
              render: (giorno: Turno["giornoSettimana"]) => ETICHETTE_GIORNO[giorno],
            },
            {
              title: "Orario",
              key: "orario",
              render: (_, turno) => `${oraBreve(turno.oraInizio)} – ${oraBreve(turno.oraFine)}`,
            },
            { title: "Durata slot (min)", dataIndex: "durataSlotMin" },
          ]}
        />
      ) : (
        <CalendarioSettimanale turni={turni} />
      )}
    </div>
  );
}
