import { useState } from "react";
import { Alert, Button, Input, Modal, Space, Typography, message } from "antd";
import { CopyOutlined } from "@ant-design/icons";

interface PasswordGenerataModalProps {
  open: boolean;
  password: string | null;
  titolo: string;
  onClose: () => void;
}

export function PasswordGenerataModal({ open, password, titolo, onClose }: PasswordGenerataModalProps) {
  const [copiato, setCopiato] = useState(false);

  const handleCopia = async () => {
    if (!password) {
      return;
    }
    try {
      await navigator.clipboard.writeText(password);
      setCopiato(true);
      message.success("Password copiata negli appunti.");
    } catch {
      message.error("Impossibile copiare la password. Selezionala e copiala manualmente.");
    }
  };

  const handleClose = () => {
    setCopiato(false);
    onClose();
  };

  return (
    <Modal
      open={open}
      title={titolo}
      onCancel={handleClose}
      footer={
        <Button type="primary" onClick={handleClose} disabled={!copiato}>
          Ho copiato la password
        </Button>
      }
      maskClosable={false}
      closable={false}
    >
      <Alert
        type="warning"
        showIcon
        message="Comunica questa password al medico: non sarà più visibile dopo la chiusura."
        style={{ marginBottom: 16 }}
      />
      <Typography.Text type="secondary">Password temporanea</Typography.Text>
      <Space.Compact style={{ width: "100%", marginTop: 4 }}>
        <Input readOnly value={password ?? ""} />
        <Button icon={<CopyOutlined />} onClick={handleCopia}>
          Copia
        </Button>
      </Space.Compact>
    </Modal>
  );
}
