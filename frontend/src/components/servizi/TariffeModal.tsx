import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Button,
  Form,
  InputNumber,
  Modal,
  Popconfirm,
  Select,
  Space,
  Spin,
  Table,
  Typography,
} from "antd";
import {
  createTariffa,
  deleteTariffa,
  getTariffePerPrestazione,
  updateTariffa,
} from "../../api/servizi";
import { getErrorMessage } from "../../api/client";
import {
  ETICHETTE_REGIME,
  Regime,
  type Prestazione,
  type Tariffa,
  type TariffaInput,
} from "../../types/servizi";

interface TariffeModalProps {
  open: boolean;
  prestazione: Prestazione | null;
  onClose: () => void;
}

type TariffaFormValues = { regime: Regime; prezzo: number };

const OPZIONI_REGIME = Object.values(Regime).map((valore) => ({
  value: valore,
  label: ETICHETTE_REGIME[valore],
}));

export function TariffeModal({ open, prestazione, onClose }: TariffeModalProps) {
  const [form] = Form.useForm<TariffaFormValues>();
  const [tariffe, setTariffe] = useState<Tariffa[]>([]);
  const [caricamento, setCaricamento] = useState(false);
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");
  const [inModifica, setInModifica] = useState<Tariffa | null>(null);

  const caricaTariffe = useCallback(async (prestazioneId: string) => {
    setCaricamento(true);
    setErrore("");
    try {
      setTariffe(await getTariffePerPrestazione(prestazioneId));
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le tariffe."));
    } finally {
      setCaricamento(false);
    }
  }, []);

  useEffect(() => {
    if (open && prestazione) {
      form.resetFields();
      setInModifica(null);
      caricaTariffe(prestazione.id);
    }
  }, [open, prestazione, form, caricaTariffe]);

  const handleSalva = async () => {
    if (!prestazione) {
      return;
    }
    let values: TariffaFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      const payload: TariffaInput = { ...values, prestazioneId: prestazione.id };
      if (inModifica) {
        await updateTariffa(inModifica.id, payload);
      } else {
        await createTariffa(payload);
      }
      form.resetFields();
      setInModifica(null);
      await caricaTariffe(prestazione.id);
    } catch (error) {
      setErrore(
        getErrorMessage(error, "Impossibile salvare la tariffa: verifica che il regime non sia già presente.")
      );
    } finally {
      setSalvataggio(false);
    }
  };

  const handleModifica = (tariffa: Tariffa) => {
    setInModifica(tariffa);
    form.setFieldsValue({ regime: tariffa.regime, prezzo: tariffa.prezzo });
  };

  const handleAnnullaModifica = () => {
    setInModifica(null);
    form.resetFields();
  };

  const handleElimina = async (tariffa: Tariffa) => {
    if (!prestazione) {
      return;
    }
    setErrore("");
    try {
      await deleteTariffa(tariffa.id);
      if (inModifica?.id === tariffa.id) {
        handleAnnullaModifica();
      }
      await caricaTariffe(prestazione.id);
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile eliminare la tariffa."));
    }
  };

  return (
    <Modal
      open={open}
      title={prestazione ? `Tariffe — ${prestazione.nome}` : "Tariffe"}
      footer={
        <Space>
          <Button type="primary" loading={salvataggio} onClick={() => form.submit()}>
            {inModifica ? "Salva" : "Aggiungi"}
          </Button>
          {inModifica && <Button onClick={handleAnnullaModifica}>Annulla modifica</Button>}
        </Space>
      }
      onCancel={onClose}
      destroyOnHidden
      width={560}
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      {caricamento ? (
        <Spin />
      ) : (
        <Table
          dataSource={tariffe}
          rowKey="id"
          pagination={false}
          size="small"
          locale={{ emptyText: "Nessuna tariffa configurata." }}
          columns={[
            {
              title: "Regime",
              dataIndex: "regime",
              render: (regime: Regime) => ETICHETTE_REGIME[regime],
            },
            {
              title: "Prezzo (€)",
              dataIndex: "prezzo",
              render: (prezzo: number) => prezzo.toFixed(2),
            },
            {
              title: "Azioni",
              key: "azioni",
              render: (_, tariffa) => (
                <Space>
                  <Button size="small" onClick={() => handleModifica(tariffa)}>
                    Modifica
                  </Button>
                  <Popconfirm
                    title="Eliminare la tariffa?"
                    okText="Elimina"
                    cancelText="Annulla"
                    onConfirm={() => handleElimina(tariffa)}
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
      )}

      <Typography.Title level={5} style={{ marginTop: 24 }}>
        {inModifica ? "Modifica tariffa" : "Aggiungi tariffa"}
      </Typography.Title>
      <Form form={form} layout="inline" onFinish={handleSalva}>
        <Form.Item name="regime" label="Regime" rules={[{ required: true, message: "Seleziona un regime." }]}>
          <Select options={OPZIONI_REGIME} style={{ width: 160 }} placeholder="Regime" />
        </Form.Item>
        <Form.Item
          name="prezzo"
          label="Prezzo"
          rules={[{ required: true, type: "number", min: 0.01, message: "Prezzo maggiore di 0." }]}
        >
          <InputNumber min={0.01} step={0.01} style={{ width: 140 }} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
