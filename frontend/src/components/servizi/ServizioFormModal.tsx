import { useEffect, useState } from "react";
import { Alert, Form, Input, Modal } from "antd";
import { createServizio, updateServizio } from "../../api/servizi";
import { getErrorMessage } from "../../api/client";
import type { Servizio, ServizioInput } from "../../types/servizi";

interface ServizioFormModalProps {
  open: boolean;
  servizio: Servizio | null;
  onClose: () => void;
  onSaved: () => void;
}

export function ServizioFormModal({ open, servizio, onClose, onSaved }: ServizioFormModalProps) {
  const [form] = Form.useForm<ServizioInput>();
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");

  useEffect(() => {
    if (open) {
      setErrore("");
      if (servizio) {
        form.setFieldsValue({ nome: servizio.nome, descrizione: servizio.descrizione });
      } else {
        form.resetFields();
      }
    }
  }, [open, servizio, form]);

  const handleOk = async () => {
    let values: ServizioInput;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      if (servizio) {
        await updateServizio(servizio.id, values);
      } else {
        await createServizio(values);
      }
      onSaved();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile salvare il servizio."));
    } finally {
      setSalvataggio(false);
    }
  };

  return (
    <Modal
      open={open}
      title={servizio ? "Modifica servizio" : "Nuovo servizio"}
      okText="Salva"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      <Form form={form} layout="vertical">
        <Form.Item
          name="nome"
          label="Nome"
          rules={[{ required: true, max: 100, message: "Nome obbligatorio (max 100 caratteri)." }]}
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
      </Form>
    </Modal>
  );
}
