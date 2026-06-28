import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Popconfirm, Space, Spin, Table, Typography } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { getErrorMessage } from "../api/client";
import { getMedici, resetPasswordMedico } from "../api/medici";
import { getServizi } from "../api/servizi";
import type { Medico, MedicoCreato } from "../types/medici";
import type { Servizio } from "../types/servizi";
import { MedicoFormModal } from "../components/medici/MedicoFormModal";
import { PasswordGenerataModal } from "../components/medici/PasswordGenerataModal";

interface PasswordModalState {
  open: boolean;
  password: string | null;
  titolo: string;
}

export function Medici() {
  const [medici, setMedici] = useState<Medico[]>([]);
  const [servizi, setServizi] = useState<Servizio[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  const [formModal, setFormModal] = useState<{ open: boolean; medico: Medico | null }>({
    open: false,
    medico: null,
  });
  const [passwordModal, setPasswordModal] = useState<PasswordModalState>({
    open: false,
    password: null,
    titolo: "",
  });

  const caricaDati = useCallback(async () => {
    setLoading(true);
    try {
      const [listaMedici, listaServizi] = await Promise.all([getMedici(), getServizi()]);
      setMedici(listaMedici);
      setServizi(listaServizi);
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare i medici."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    caricaDati();
  }, [caricaDati]);

  const handleCreated = (creato: MedicoCreato) => {
    setFormModal({ open: false, medico: null });
    setPasswordModal({ open: true, password: creato.passwordGenerata, titolo: "Medico creato" });
    caricaDati();
  };

  const handleUpdated = () => {
    setFormModal({ open: false, medico: null });
    caricaDati();
  };

  const handleResetPassword = async (medico: Medico) => {
    setErrore("");
    try {
      const risultato = await resetPasswordMedico(medico.id);
      setPasswordModal({
        open: true,
        password: risultato.passwordGenerata,
        titolo: "Password reimpostata",
      });
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile reimpostare la password."));
    }
  };

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={2} style={{ margin: 0 }}>
          Medici
        </Typography.Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setFormModal({ open: true, medico: null })}
        >
          Nuovo medico
        </Button>
      </Space>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Table
        dataSource={medici}
        rowKey="id"
        pagination={false}
        locale={{ emptyText: "Nessun medico registrato." }}
        columns={[
          { title: "Nome", dataIndex: "nome" },
          { title: "Cognome", dataIndex: "cognome" },
          { title: "Email", dataIndex: "email" },
          { title: "Specializzazione", dataIndex: "specializzazione" },
          { title: "Servizio", dataIndex: "servizioNome" },
          {
            title: "Azioni",
            key: "azioni",
            render: (_, medico) => (
              <Space>
                <Button size="small" onClick={() => setFormModal({ open: true, medico })}>
                  Modifica
                </Button>
                <Popconfirm
                  title="Reimpostare la password?"
                  description="La password attuale non sarà più valida."
                  okText="Reimposta"
                  cancelText="Annulla"
                  onConfirm={() => handleResetPassword(medico)}
                >
                  <Button size="small">Reset password</Button>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />

      <MedicoFormModal
        open={formModal.open}
        medico={formModal.medico}
        servizi={servizi}
        onClose={() => setFormModal({ open: false, medico: null })}
        onCreated={handleCreated}
        onUpdated={handleUpdated}
      />
      <PasswordGenerataModal
        open={passwordModal.open}
        password={passwordModal.password}
        titolo={passwordModal.titolo}
        onClose={() => setPasswordModal({ open: false, password: null, titolo: "" })}
      />
    </div>
  );
}
