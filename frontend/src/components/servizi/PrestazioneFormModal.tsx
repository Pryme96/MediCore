import { useEffect, useState } from "react";
import { Alert, Form, Input, InputNumber, Modal } from "antd";
import { createPrestazione, updatePrestazione } from "../../api/servizi";
import { getErrorMessage } from "../../api/client";
import type { Prestazione, PrestazioneInput } from "../../types/servizi";

interface PrestazioneFormModalProps {
  open: boolean;
  servizioId: string;
  prestazione: Prestazione | null;
  onClose: () => void;
  onSaved: () => void;
}

type PrestazioneFormValues = Omit<PrestazioneInput, "servizioId">;

export function PrestazioneFormModal({
  open,
  servizioId,
  prestazione,
  onClose,
  onSaved,
}: PrestazioneFormModalProps) {
  const [form] = Form.useForm<PrestazioneFormValues>();
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");

  useEffect(() => {
    if (open) {
      setErrore("");
      if (prestazione) {
        form.setFieldsValue({
          nome: prestazione.nome,
          descrizione: prestazione.descrizione,
          durataMinuti: prestazione.durataMinuti,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, prestazione, form]);

  const handleOk = async () => {
    let values: PrestazioneFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      const payload: PrestazioneInput = { ...values, servizioId };
      if (prestazione) {
        await updatePrestazione(prestazione.id, payload);
      } else {
        await createPrestazione(payload);
      }
      onSaved();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile salvare la prestazione."));
    } finally {
      setSalvataggio(false);
    }
  };

  return (
    <Modal
      open={open}
      title={prestazione ? "Modifica prestazione" : "Nuova prestazione"}
      okText="Salva"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      <Form form={form} layout="vertical" initialValues={{ durataMinuti: 30 }}>
        <Form.Item
          name="nome"
          label="Nome"
          rules={[{ required: true, max: 150, message: "Nome obbligatorio (max 150 caratteri)." }]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="descrizione"
          label="Descrizione"
          rules={[{ required: true, max: 500, message: "Descrizione obbligatoria (max 500 caratteri)." }]}
        >
          <Input.TextArea rows={3} />
        </Form.Item>
        <Form.Item
          name="durataMinuti"
          label="Durata (minuti)"
          rules={[{ required: true, type: "number", min: 1, max: 600, message: "Durata tra 1 e 600 minuti." }]}
        >
          <InputNumber min={1} max={600} style={{ width: "100%" }} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
