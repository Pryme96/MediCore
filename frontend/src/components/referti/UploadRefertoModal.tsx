import { useEffect, useState } from "react";
import { Alert, Button, Form, Input, Modal, Typography, Upload } from "antd";
import { InboxOutlined } from "@ant-design/icons";
import type { UploadFile } from "antd";
import dayjs from "dayjs";
import { uploadReferto } from "../../api/referti";
import { getErrorMessage } from "../../api/client";
import type { Prenotazione } from "../../types/prenotazioni";

interface UploadRefertoModalProps {
  open: boolean;
  prenotazione: Prenotazione | null;
  refertoEsistente: boolean;
  onClose: () => void;
  onCaricato: () => void;
}

const DIMENSIONE_MAX = 10_000_000; // 10 MB, allineato al limite del backend.

export function UploadRefertoModal({
  open,
  prenotazione,
  refertoEsistente,
  onClose,
  onCaricato,
}: UploadRefertoModalProps) {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [contenuto, setContenuto] = useState("");
  const [salvataggio, setSalvataggio] = useState(false);
  const [errore, setErrore] = useState("");

  useEffect(() => {
    if (open) {
      setFileList([]);
      setContenuto("");
      setErrore("");
    }
  }, [open]);

  const handleOk = async () => {
    const file = fileList[0]?.originFileObj;
    if (!file) {
      setErrore("Seleziona un file PDF da caricare.");
      return;
    }
    if (!prenotazione) {
      return;
    }
    setSalvataggio(true);
    setErrore("");
    try {
      await uploadReferto(prenotazione.id, file, contenuto.trim() || undefined);
      onCaricato();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare il referto."));
    } finally {
      setSalvataggio(false);
    }
  };

  return (
    <Modal
      open={open}
      title={refertoEsistente ? "Sostituisci referto" : "Carica referto"}
      okText="Carica"
      cancelText="Annulla"
      confirmLoading={salvataggio}
      onOk={handleOk}
      onCancel={onClose}
      destroyOnClose
    >
      {prenotazione && (
        <Typography.Paragraph type="secondary" style={{ marginBottom: 16 }}>
          {prenotazione.prestazioneNome} — {prenotazione.pazienteNomeCompleto},{" "}
          {dayjs(prenotazione.dataOraInizio).format("DD/MM/YYYY HH:mm")}
        </Typography.Paragraph>
      )}

      {refertoEsistente && (
        <Alert
          type="warning"
          message="Esiste già un referto per questa prenotazione: il caricamento lo sostituirà."
          style={{ marginBottom: 16 }}
        />
      )}

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      <Form layout="vertical">
        <Form.Item label="File referto (PDF)" required>
          <Upload.Dragger
            accept="application/pdf"
            maxCount={1}
            fileList={fileList}
            beforeUpload={(file) => {
              if (file.type !== "application/pdf") {
                setErrore("Il file deve essere un PDF.");
                return Upload.LIST_IGNORE;
              }
              if (file.size > DIMENSIONE_MAX) {
                setErrore("Il file supera la dimensione massima di 10 MB.");
                return Upload.LIST_IGNORE;
              }
              setErrore("");
              setFileList([{ uid: file.uid, name: file.name, originFileObj: file }]);
              return false;
            }}
            onRemove={() => setFileList([])}
          >
            <p className="ant-upload-drag-icon">
              <InboxOutlined />
            </p>
            <p className="ant-upload-text">Trascina qui il PDF o clicca per selezionarlo</p>
          </Upload.Dragger>
        </Form.Item>

        <Form.Item label="Note (facoltative)">
          <Input.TextArea
            rows={3}
            value={contenuto}
            onChange={(e) => setContenuto(e.target.value)}
            maxLength={1000}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
}
