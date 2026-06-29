import { useMemo } from "react";
import { Card, Col, Row, Space, Tag, Typography } from "antd";
import {
  MedicineBoxOutlined,
  CalendarOutlined,
  FileTextOutlined,
  FilePdfOutlined,
  EuroCircleOutlined,
  TeamOutlined,
  ScheduleOutlined,
  AppstoreOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { palette } from "../theme/colors";

interface Scorciatoia {
  titolo: string;
  descrizione: string;
  path: string;
  icona: React.ReactNode;
  ruoli: string[];
}

// Card di accesso rapido mostrate in home, filtrate per ruolo (stesse sezioni del menu laterale).
const SCORCIATOIE: Scorciatoia[] = [
  { titolo: "Gestione Servizi", descrizione: "Servizi, prestazioni e tariffe.", path: "/gestione-servizi", icona: <AppstoreOutlined />, ruoli: ["Amministratore"] },
  { titolo: "Servizi", descrizione: "Servizi e prestazioni disponibili.", path: "/servizi", icona: <MedicineBoxOutlined />, ruoli: ["Paziente", "Medico"] },
  { titolo: "Prenotazioni", descrizione: "Appuntamenti e prenotazioni.", path: "/prenotazioni", icona: <CalendarOutlined />, ruoli: ["Paziente", "Amministratore", "Medico"] },
  { titolo: "Prescrizioni", descrizione: "Prescrizioni e piani terapeutici.", path: "/prescrizioni", icona: <FileTextOutlined />, ruoli: ["Paziente", "Medico"] },
  { titolo: "Referti", descrizione: "Referti clinici in PDF.", path: "/referti", icona: <FilePdfOutlined />, ruoli: ["Paziente", "Medico"] },
  { titolo: "Fatture", descrizione: "Fatture e importi.", path: "/fatture", icona: <EuroCircleOutlined />, ruoli: ["Paziente", "Amministratore"] },
  { titolo: "Medici", descrizione: "Anagrafica dei medici.", path: "/medici", icona: <TeamOutlined />, ruoli: ["Amministratore"] },
  { titolo: "Turni", descrizione: "Turni e disponibilità.", path: "/turni", icona: <ScheduleOutlined />, ruoli: ["Amministratore", "Medico"] },
];

export function Home() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const scorciatoieVisibili = useMemo(
    () => SCORCIATOIE.filter((voce) => voce.ruoli.some((ruolo) => user?.ruoli.includes(ruolo))),
    [user]
  );

  const nomeCompleto = [user?.nome, user?.cognome].filter(Boolean).join(" ").trim();

  return (
    <div>
      <Space direction="vertical" size={4} style={{ marginBottom: 24 }}>
        <Typography.Title level={2} style={{ marginBottom: 0 }}>
          Benvenuto{nomeCompleto ? `, ${nomeCompleto}` : " in MediCore"}
        </Typography.Title>
        <Space size={8} wrap>
          <Typography.Text type="secondary">{user?.email}</Typography.Text>
          {user?.ruoli.map((ruolo) => (
            <Tag key={ruolo} style={{ background: palette.primary, color: "#fff", border: "none" }}>
              {ruolo}
            </Tag>
          ))}
        </Space>
      </Space>

      <Typography.Paragraph type="secondary" style={{ marginBottom: 24 }}>
        Seleziona una sezione per iniziare.
      </Typography.Paragraph>

      <Row gutter={[16, 16]}>
        {scorciatoieVisibili.map((voce) => (
          <Col key={voce.path} xs={24} sm={12} md={8} lg={6}>
            <Card hoverable onClick={() => navigate(voce.path)} style={{ height: "100%" }}>
              <Space align="start" size={16}>
                <span style={{ fontSize: 28, color: palette.primary, lineHeight: 1 }}>{voce.icona}</span>
                <Space direction="vertical" size={2}>
                  <Typography.Text strong>{voce.titolo}</Typography.Text>
                  <Typography.Text type="secondary" style={{ fontSize: 13 }}>
                    {voce.descrizione}
                  </Typography.Text>
                </Space>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );
}
