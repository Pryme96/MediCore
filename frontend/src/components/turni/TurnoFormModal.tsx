import { useEffect, useState } from "react";
import { Alert, Form, InputNumber, Modal, Select, TimePicker } from "antd";
import dayjs, { type Dayjs } from "dayjs";
import { createTurno, updateTurno } from "../../api/turni";
import { getErrorMessage } from "../../api/client";
import { ETICHETTE_GIORNO, GiornoSettimana, type Turno } from "../../types/turni";
import type { Medico } from "../../types/medici";
import type { Prestazione } from "../../types/servizi";

interface TurnoFormModalProps {
  open: boolean;
  turno: Turno | null;
  medici: Medico[];
  prestazioni: Prestazione[];
  onClose: () => void;
  onSaved: () => void;
}

interface TurnoFormValues {
  medicoId: string;
  prestazioneId: string;
  giornoSettimana: GiornoSettimana;
  oraInizio: Dayjs;
  oraFine: Dayjs;
  durataSlotMin: number;
}

const FORMATO_ORA = "HH:mm";
const ORA_APERTURA = 8;
const ORA_CHIUSURA = 20;

// L'ambulatorio è aperto 08:00–20:00: fuori da questa fascia gli orari sono disabilitati.
const orariConsentiti = () => ({
  disabledHours: () =>
    Array.from({ length: 24 }, (_, h) => h).filter((h) => h < ORA_APERTURA || h > ORA_CHIUSURA),
  disabledMinutes: (oraSelezionata: number) =>
    oraSelezionata === ORA_CHIUSURA ? Array.from({ length: 60 }, (_, m) => m).filter((m) => m !== 0) : [],
});

// La response espone gli orari come stringa "HH:mm:ss"; per il TimePicker servono Dayjs.
const oraToDayjs = (ora: string): Dayjs => {
  const [h, m, s] = ora.split(":").map(Number);
  return dayjs().hour(h).minute(m).second(s ?? 0);
};

const OPZIONI_GIORNO = Object.values(GiornoSettimana).map((valore) => ({
  value: valore,
  label: ETICHETTE_GIORNO[valore],
}));

export function TurnoFormModal({
  open,
  turno,
  medici,
  prestazioni,
  onClose,
  onSaved,
}: TurnoFormModalProps) {
  const [form] = Form.useForm<TurnoFormValues>();
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");

  useEffect(() => {
    if (open) {
      setErrore("");
      if (turno) {
        form.setFieldsValue({
          medicoId: turno.medicoId,
          prestazioneId: turno.prestazioneId,
          giornoSettimana: turno.giornoSettimana,
          oraInizio: oraToDayjs(turno.oraInizio),
          oraFine: oraToDayjs(turno.oraFine),
          durataSlotMin: turno.durataSlotMin,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, turno, form]);

  const handleOk = async () => {
    let values: TurnoFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      const payload = {
        medicoId: values.medicoId,
        prestazioneId: values.prestazioneId,
        giornoSettimana: values.giornoSettimana,
        oraInizio: values.oraInizio.format("HH:mm:ss"),
        oraFine: values.oraFine.format("HH:mm:ss"),
        durataSlotMin: values.durataSlotMin,
      };
      if (turno) {
        await updateTurno(turno.id, payload);
      } else {
        await createTurno(payload);
      }
      onSaved();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile salvare il turno: verifica i dati inseriti."));
    } finally {
      setSalvataggio(false);
    }
  };

  return (
    <Modal
      open={open}
      title={turno ? "Modifica turno" : "Nuovo turno"}
      okText="Salva"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      <Form form={form} layout="vertical" initialValues={{ durataSlotMin: 30 }}>
        <Form.Item name="medicoId" label="Medico" rules={[{ required: true, message: "Seleziona un medico." }]}>
          <Select
            placeholder="Seleziona un medico"
            options={medici.map((m) => ({
              value: m.id,
              label: `${m.cognome} ${m.nome} — ${m.specializzazione}`,
            }))}
          />
        </Form.Item>
        <Form.Item
          name="prestazioneId"
          label="Prestazione"
          rules={[{ required: true, message: "Seleziona una prestazione." }]}
        >
          <Select
            placeholder="Seleziona una prestazione"
            options={prestazioni.map((p) => ({ value: p.id, label: `${p.nome} (${p.servizioNome})` }))}
          />
        </Form.Item>
        <Form.Item
          name="giornoSettimana"
          label="Giorno"
          rules={[{ required: true, message: "Seleziona un giorno." }]}
        >
          <Select placeholder="Seleziona un giorno" options={OPZIONI_GIORNO} />
        </Form.Item>
        <Form.Item
          name="oraInizio"
          label="Ora inizio"
          rules={[{ required: true, message: "Indica l'ora di inizio." }]}
        >
          <TimePicker
            format={FORMATO_ORA}
            minuteStep={5}
            disabledTime={orariConsentiti}
            hideDisabledOptions
            style={{ width: "100%" }}
          />
        </Form.Item>
        <Form.Item
          name="oraFine"
          label="Ora fine"
          dependencies={["oraInizio"]}
          rules={[
            { required: true, message: "Indica l'ora di fine." },
            ({ getFieldValue }) => ({
              validator(_, value: Dayjs) {
                const inizio = getFieldValue("oraInizio") as Dayjs | undefined;
                if (!value || !inizio || value.isAfter(inizio)) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error("L'ora di fine deve essere successiva all'ora di inizio."));
              },
            }),
          ]}
        >
          <TimePicker
            format={FORMATO_ORA}
            minuteStep={5}
            disabledTime={orariConsentiti}
            hideDisabledOptions
            style={{ width: "100%" }}
          />
        </Form.Item>
        <Form.Item
          name="durataSlotMin"
          label="Durata slot (minuti)"
          rules={[{ required: true, type: "number", min: 1, max: 480, message: "Durata tra 1 e 480 minuti." }]}
        >
          <InputNumber min={1} max={480} style={{ width: "100%" }} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
