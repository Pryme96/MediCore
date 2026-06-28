import { useEffect, useState } from "react";
import { Alert, Form, Input, Modal, Select } from "antd";
import { createMedico, updateMedico } from "../../api/medici";
import { getErrorMessage } from "../../api/client";
import type { Medico, MedicoCreato } from "../../types/medici";
import type { Servizio } from "../../types/servizi";

interface MedicoFormModalProps {
  open: boolean;
  medico: Medico | null;
  servizi: Servizio[];
  onClose: () => void;
  onCreated: (creato: MedicoCreato) => void;
  onUpdated: () => void;
}

interface MedicoFormValues {
  email: string;
  nome: string;
  cognome: string;
  specializzazione: string;
  servizioId: string;
}

export function MedicoFormModal({
  open,
  medico,
  servizi,
  onClose,
  onCreated,
  onUpdated,
}: MedicoFormModalProps) {
  const [form] = Form.useForm<MedicoFormValues>();
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");

  const inModifica = medico !== null;

  useEffect(() => {
    if (open) {
      setErrore("");
      if (medico) {
        form.setFieldsValue({
          email: medico.email,
          nome: medico.nome,
          cognome: medico.cognome,
          specializzazione: medico.specializzazione,
          servizioId: medico.servizioId,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, medico, form]);

  const handleOk = async () => {
    let values: MedicoFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      if (medico) {
        await updateMedico(medico.id, {
          specializzazione: values.specializzazione,
          servizioId: values.servizioId,
        });
        onUpdated();
      } else {
        const creato = await createMedico(values);
        onCreated(creato);
      }
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile salvare il medico: verifica i dati inseriti."));
    } finally {
      setSalvataggio(false);
    }
  };

  const opzioniServizi = servizi.map((servizio) => ({
    value: servizio.id,
    label: servizio.nome,
  }));

  return (
    <Modal
      open={open}
      title={inModifica ? "Modifica medico" : "Nuovo medico"}
      okText="Salva"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
    >
      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}
      {!inModifica && servizi.length === 0 && (
        <Alert
          type="warning"
          showIcon
          message="Nessun servizio configurato: crea prima un servizio dalla sezione Gestione Servizi."
          style={{ marginBottom: 16 }}
        />
      )}
      <Form form={form} layout="vertical">
        <Form.Item
          name="nome"
          label="Nome"
          rules={[{ required: true, message: "Nome obbligatorio." }]}
        >
          <Input disabled={inModifica} />
        </Form.Item>
        <Form.Item
          name="cognome"
          label="Cognome"
          rules={[{ required: true, message: "Cognome obbligatorio." }]}
        >
          <Input disabled={inModifica} />
        </Form.Item>
        <Form.Item
          name="email"
          label="Email"
          rules={[{ required: true, type: "email", message: "Email valida obbligatoria." }]}
        >
          <Input disabled={inModifica} />
        </Form.Item>
        <Form.Item
          name="specializzazione"
          label="Specializzazione"
          rules={[{ required: true, message: "Specializzazione obbligatoria." }]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="servizioId"
          label="Servizio"
          rules={[{ required: true, message: "Seleziona un servizio." }]}
        >
          <Select options={opzioniServizi} placeholder="Seleziona un servizio" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
