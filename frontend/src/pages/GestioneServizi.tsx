import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Collapse, List, Space, Spin, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { getErrorMessage } from "../api/client";
import { getPrestazioniPerServizio, getServizi } from "../api/servizi";
import type { Prestazione, Servizio } from "../types/servizi";
import { ServizioFormModal } from "../components/servizi/ServizioFormModal";
import { PrestazioneFormModal } from "../components/servizi/PrestazioneFormModal";
import { TariffeModal } from "../components/servizi/TariffeModal";

interface ServizioModalState {
  open: boolean;
  servizio: Servizio | null;
}

interface PrestazioneModalState {
  open: boolean;
  servizioId: string;
  prestazione: Prestazione | null;
}

interface TariffeModalState {
  open: boolean;
  prestazione: Prestazione | null;
}

export function GestioneServizi() {
  const [servizi, setServizi] = useState<Servizio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");
  const [prestazioniPerServizio, setPrestazioniPerServizio] = useState<
    Record<string, Prestazione[]>
  >({});
  const [prestazioniLoading, setPrestazioniLoading] = useState<Record<string, boolean>>({});

  const [servizioModal, setServizioModal] = useState<ServizioModalState>({
    open: false,
    servizio: null,
  });
  const [prestazioneModal, setPrestazioneModal] = useState<PrestazioneModalState>({
    open: false,
    servizioId: "",
    prestazione: null,
  });
  const [tariffeModal, setTariffeModal] = useState<TariffeModalState>({
    open: false,
    prestazione: null,
  });

  const caricaServizi = useCallback(async () => {
    setLoading(true);
    try {
      setServizi(await getServizi());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare i servizi."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    caricaServizi();
  }, [caricaServizi]);

  const caricaPrestazioni = useCallback(async (servizioId: string) => {
    setPrestazioniLoading((prev) => ({ ...prev, [servizioId]: true }));
    try {
      const prestazioni = await getPrestazioniPerServizio(servizioId);
      setPrestazioniPerServizio((prev) => ({ ...prev, [servizioId]: prestazioni }));
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le prestazioni."));
    } finally {
      setPrestazioniLoading((prev) => ({ ...prev, [servizioId]: false }));
    }
  }, []);

  const handlePannelloAperto = (servizioId: string) => {
    if (prestazioniPerServizio[servizioId] || prestazioniLoading[servizioId]) {
      return;
    }
    caricaPrestazioni(servizioId);
  };

  const handleServizioSalvato = () => {
    setServizioModal({ open: false, servizio: null });
    caricaServizi();
  };

  const handlePrestazioneSalvata = () => {
    const servizioId = prestazioneModal.servizioId;
    setPrestazioneModal({ open: false, servizioId: "", prestazione: null });
    if (servizioId) {
      caricaPrestazioni(servizioId);
    }
  };

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          Gestione Servizi
        </Typography.Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setServizioModal({ open: true, servizio: null })}
        >
          Nuovo servizio
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Collapse
        onChange={(keys) => {
          const aperti = Array.isArray(keys) ? keys : [keys];
          aperti.forEach((key) => handlePannelloAperto(key));
        }}
        items={servizi.map((servizio) => ({
          key: servizio.id,
          label: servizio.nome,
          extra: (
            <Button
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                setServizioModal({ open: true, servizio });
              }}
            >
              Modifica
            </Button>
          ),
          children: (
            <>
              <Space style={{ width: "100%", justifyContent: "flex-end", marginBottom: 8 }}>
                <Button
                  size="small"
                  icon={<PlusOutlined />}
                  onClick={() =>
                    setPrestazioneModal({
                      open: true,
                      servizioId: servizio.id,
                      prestazione: null,
                    })
                  }
                >
                  Nuova prestazione
                </Button>
              </Space>
              {prestazioniLoading[servizio.id] ? (
                <Spin size="small" />
              ) : (
                <List
                  dataSource={prestazioniPerServizio[servizio.id] ?? []}
                  locale={{ emptyText: "Nessuna prestazione per questo servizio." }}
                  renderItem={(prestazione) => (
                    <List.Item
                      actions={[
                        <Button
                          key="modifica"
                          size="small"
                          onClick={() =>
                            setPrestazioneModal({
                              open: true,
                              servizioId: servizio.id,
                              prestazione,
                            })
                          }
                        >
                          Modifica
                        </Button>,
                        <Button
                          key="tariffe"
                          size="small"
                          onClick={() => setTariffeModal({ open: true, prestazione })}
                        >
                          Tariffe
                        </Button>,
                      ]}
                    >
                      <List.Item.Meta
                        title={prestazione.nome}
                        description={`${prestazione.descrizione} — ${prestazione.durataMinuti} min`}
                      />
                    </List.Item>
                  )}
                />
              )}
            </>
          ),
        }))}
      />

      <ServizioFormModal
        open={servizioModal.open}
        servizio={servizioModal.servizio}
        onClose={() => setServizioModal({ open: false, servizio: null })}
        onSaved={handleServizioSalvato}
      />
      <PrestazioneFormModal
        open={prestazioneModal.open}
        servizioId={prestazioneModal.servizioId}
        prestazione={prestazioneModal.prestazione}
        onClose={() => setPrestazioneModal({ open: false, servizioId: "", prestazione: null })}
        onSaved={handlePrestazioneSalvata}
      />
      <TariffeModal
        open={tariffeModal.open}
        prestazione={tariffeModal.prestazione}
        onClose={() => setTariffeModal({ open: false, prestazione: null })}
      />
    </div>
  );
}
