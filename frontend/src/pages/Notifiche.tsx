import { useCallback, useEffect, useState } from "react";
import { Alert, Badge, Button, Empty, List, Space, Spin, Tag, Typography } from "antd";
import dayjs from "dayjs";
import { getErrorMessage } from "../api/client";
import { getNotificheMie, marcaNotificaLetta } from "../api/notifiche";
import { confermaPresenza } from "../api/prenotazioni";
import {
  ETICHETTE_TIPO_NOTIFICA,
  TipoNotifica,
  type Notifica,
} from "../types/notifiche";

const COLORE_TIPO: Record<TipoNotifica, string> = {
  [TipoNotifica.PromemoriaAppuntamento]: "gold",
  [TipoNotifica.Prescrizione]: "blue",
};

export function Notifiche() {
  const [notifiche, setNotifiche] = useState<Notifica[]>([]);
  const [loading, setLoading] = useState(true);
  const [errore, setErrore] = useState("");

  const carica = useCallback(async () => {
    setLoading(true);
    try {
      setNotifiche(await getNotificheMie());
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile caricare le notifiche."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carica();
  }, [carica]);

  const handleLetta = async (n: Notifica) => {
    setErrore("");
    try {
      await marcaNotificaLetta(n.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile aggiornare la notifica."));
    }
  };

  const handleConferma = async (n: Notifica) => {
    if (!n.riferimentoId) return;
    setErrore("");
    try {
      await confermaPresenza(n.riferimentoId);
      if (!n.letta) await marcaNotificaLetta(n.id);
      carica();
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile confermare la presenza: la prenotazione potrebbe non essere più confermabile."));
    }
  };

  if (loading) {
    return <Spin />;
  }

  return (
    <div>
      <Typography.Title level={2} style={{ marginTop: 0, marginBottom: 16 }}>
        Notifiche
      </Typography.Title>

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} />}

      {notifiche.length === 0 ? (
        <Empty description="Non hai notifiche." />
      ) : (
        <List
          dataSource={notifiche}
          rowKey="id"
          renderItem={(n) => (
            <List.Item
              style={{ background: n.letta ? undefined : "rgba(15, 152, 157, 0.06)", paddingInline: 16 }}
              actions={[
                ...(n.tipo === TipoNotifica.PromemoriaAppuntamento && n.riferimentoId
                  ? [
                      <Button key="conferma" size="small" type="primary" onClick={() => handleConferma(n)}>
                        Conferma presenza
                      </Button>,
                    ]
                  : []),
                ...(n.letta
                  ? []
                  : [
                      <Button key="letta" size="small" onClick={() => handleLetta(n)}>
                        Segna come letta
                      </Button>,
                    ]),
              ]}
            >
              <List.Item.Meta
                title={
                  <Space>
                    {!n.letta && <Badge status="processing" />}
                    <Tag color={COLORE_TIPO[n.tipo]}>{ETICHETTE_TIPO_NOTIFICA[n.tipo]}</Tag>
                    <span>{n.titolo}</span>
                  </Space>
                }
                description={
                  <div>
                    <div>{n.messaggio}</div>
                    <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                      {dayjs(n.dataCreazione).format("DD/MM/YYYY HH:mm")}
                    </Typography.Text>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      )}
    </div>
  );
}
