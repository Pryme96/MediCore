import { useEffect, useMemo, useState } from "react";
import { Alert, Button, DatePicker, Divider, Form, Input, InputNumber, Modal, Segmented, Select, Space } from "antd";
import { MinusCircleOutlined, PlusOutlined } from "@ant-design/icons";
import type { Dayjs } from "dayjs";
import dayjs from "dayjs";
import { createPrescrizione } from "../../api/prescrizioni";
import { getPrenotazioniAgenda } from "../../api/prenotazioni";
import { getErrorMessage } from "../../api/client";
import type { Prenotazione } from "../../types/prenotazioni";
import { TipoPrescrizione, type RigaPrescrizione } from "../../types/prescrizioni";

interface Props {
  open: boolean;
  onClose: () => void;
  onCreata: () => void;
}

interface PrescrizioneFormValues {
  pazienteId: string;
  tipo: TipoPrescrizione;
  diagnosi?: string;
  durataGiorni?: number;
  monitoraggio?: string;
  dataEmissione: Dayjs;
  dataScadenza: Dayjs;
  note?: string;
  righe: RigaPrescrizione[];
}

export function PrescrizioneFormModal({ open, onClose, onCreata }: Props) {
  const [form] = Form.useForm<PrescrizioneFormValues>();
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");
  const [agenda, setAgenda] = useState<Prenotazione[]>([]);
  const [caricamentoPazienti, setCaricamentoPazienti] = useState(false);

  const tipo = Form.useWatch("tipo", form);
  const isPianoTerapeutico = tipo === TipoPrescrizione.PianoTerapeutico;

  // L'elenco dei pazienti selezionabili è derivato dall'agenda del medico (prenotazioni sui
  // suoi turni): coincide con la regola backend "almeno una prenotazione pregressa", così non
  // si possono selezionare pazienti che produrrebbero un 400 in fase di salvataggio.
  useEffect(() => {
    if (!open) return;
    setErrore("");
    form.resetFields();
    form.setFieldsValue({
      tipo: TipoPrescrizione.Farmacologica,
      dataEmissione: dayjs(),
      righe: [{ farmaco: "", posologia: "", quantita: 1 }],
    });

    let attivo = true;
    setCaricamentoPazienti(true);
    getPrenotazioniAgenda()
      .then((prenotazioni) => {
        if (attivo) setAgenda(prenotazioni);
      })
      .catch((error) => {
        if (attivo) setErrore(getErrorMessage(error, "Impossibile caricare l'elenco dei pazienti."));
      })
      .finally(() => {
        if (attivo) setCaricamentoPazienti(false);
      });
    return () => {
      attivo = false;
    };
  }, [open, form]);

  const opzioniPazienti = useMemo(() => {
    const visti = new Map<string, string>();
    for (const p of agenda) {
      if (!visti.has(p.pazienteId)) visti.set(p.pazienteId, p.pazienteNomeCompleto);
    }
    return Array.from(visti, ([value, label]) => ({ value, label }));
  }, [agenda]);

  const handleOk = async () => {
    let values: PrescrizioneFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      await createPrescrizione({
        pazienteId: values.pazienteId,
        tipo: values.tipo,
        diagnosi: values.diagnosi?.trim() ? values.diagnosi : undefined,
        durataGiorni: isPianoTerapeutico ? values.durataGiorni : undefined,
        monitoraggio: isPianoTerapeutico && values.monitoraggio?.trim() ? values.monitoraggio : undefined,
        dataEmissione: values.dataEmissione.format("YYYY-MM-DD"),
        dataScadenza: values.dataScadenza.format("YYYY-MM-DD"),
        note: values.note?.trim() ? values.note : undefined,
        righe: values.righe,
      });
      onCreata();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile salvare la prescrizione: verifica i dati inseriti."));
    } finally {
      setSalvataggio(false);
    }
  };

  return (
    <Modal
      open={open}
      title="Nuova prescrizione"
      okText="Salva"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
      width={720}
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      {!caricamentoPazienti && opzioniPazienti.length === 0 && (
        <Alert
          type="warning"
          showIcon
          message="Nessun paziente disponibile: puoi prescrivere solo a pazienti con cui hai avuto almeno una prenotazione."
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Spazio riservato all'assistenza alla redazione clinica (card di suggerimento):
          al passo successivo popolerà diagnosi e righe dei farmaci tramite form.setFieldsValue. */}

      <Form form={form} layout="vertical">
        <Form.Item name="tipo" label="Tipo">
          <Segmented
            options={[
              { label: "Farmacologica", value: TipoPrescrizione.Farmacologica },
              { label: "Piano terapeutico", value: TipoPrescrizione.PianoTerapeutico },
            ]}
          />
        </Form.Item>

        <Form.Item
          name="pazienteId"
          label="Paziente"
          rules={[{ required: true, message: "Seleziona un paziente." }]}
        >
          <Select
            options={opzioniPazienti}
            loading={caricamentoPazienti}
            placeholder="Seleziona un paziente"
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>

        <Form.Item
          name="diagnosi"
          label="Diagnosi / indicazione clinica"
          rules={
            isPianoTerapeutico
              ? [{ required: true, message: "La diagnosi è obbligatoria per il piano terapeutico." }]
              : []
          }
        >
          <Input.TextArea rows={2} placeholder="Indicazione clinica" />
        </Form.Item>

        <Space style={{ width: "100%" }} size="large">
          <Form.Item
            name="dataEmissione"
            label="Data emissione"
            rules={[{ required: true, message: "Data di emissione obbligatoria." }]}
          >
            <DatePicker format="DD/MM/YYYY" style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item
            name="dataScadenza"
            label="Data scadenza"
            dependencies={["dataEmissione"]}
            rules={[
              { required: true, message: "Data di scadenza obbligatoria." },
              ({ getFieldValue }) => ({
                validator(_, value: Dayjs) {
                  const emissione = getFieldValue("dataEmissione") as Dayjs | undefined;
                  if (!value || !emissione || value.isAfter(emissione, "day")) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error("La scadenza deve essere successiva all'emissione."));
                },
              }),
            ]}
          >
            <DatePicker format="DD/MM/YYYY" style={{ width: "100%" }} />
          </Form.Item>
          {isPianoTerapeutico && (
            <Form.Item name="durataGiorni" label="Durata (giorni)">
              <InputNumber min={1} max={365} style={{ width: "100%" }} placeholder="es. 180" />
            </Form.Item>
          )}
        </Space>

        {isPianoTerapeutico && (
          <Form.Item name="monitoraggio" label="Modalità di monitoraggio">
            <Input.TextArea rows={2} placeholder="es. controllo pressorio mensile" />
          </Form.Item>
        )}

        <Divider orientation="left" style={{ margin: "8px 0 16px" }}>
          Farmaci
        </Divider>

        <Form.List name="righe">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...rest }) => (
                <Space key={key} align="baseline" style={{ display: "flex", marginBottom: 8 }}>
                  <Form.Item
                    {...rest}
                    name={[name, "farmaco"]}
                    rules={[{ required: true, message: "Indica il farmaco." }]}
                    style={{ marginBottom: 0 }}
                  >
                    <Input placeholder="Farmaco" style={{ width: 240 }} />
                  </Form.Item>
                  <Form.Item
                    {...rest}
                    name={[name, "posologia"]}
                    rules={[{ required: true, message: "Indica la posologia." }]}
                    style={{ marginBottom: 0 }}
                  >
                    <Input placeholder="Posologia" style={{ width: 240 }} />
                  </Form.Item>
                  <Form.Item
                    {...rest}
                    name={[name, "quantita"]}
                    rules={[{ required: true, message: "Quantità." }]}
                    style={{ marginBottom: 0 }}
                  >
                    <InputNumber min={1} placeholder="Q.tà" style={{ width: 80 }} />
                  </Form.Item>
                  {fields.length > 1 && (
                    <MinusCircleOutlined onClick={() => remove(name)} style={{ color: "#999" }} />
                  )}
                </Space>
              ))}
              <Form.Item style={{ marginTop: 8 }}>
                <Button type="dashed" onClick={() => add({ farmaco: "", posologia: "", quantita: 1 })} icon={<PlusOutlined />} block>
                  Aggiungi farmaco
                </Button>
              </Form.Item>
            </>
          )}
        </Form.List>

        <Form.Item name="note" label="Note">
          <Input.TextArea rows={2} placeholder="Note aggiuntive (facoltative)" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
