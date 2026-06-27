import { useMemo } from "react";
import { Layout, Menu, Button, Typography, Card } from "antd";
import {
  MedicineBoxOutlined,
  CalendarOutlined,
  FileTextOutlined,
  FilePdfOutlined,
  EuroCircleOutlined,
  TeamOutlined,
  ScheduleOutlined,
} from "@ant-design/icons";
import { Link, Outlet, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { palette } from "../theme/colors";

const { Header, Sider, Content } = Layout;

interface MenuVoce {
  key: string;
  label: string;
  path: string;
  icona: React.ReactNode;
  ruoli?: string[];
}

const VOCI_MENU: MenuVoce[] = [
  { key: "servizi", label: "Servizi", path: "/servizi", icona: <MedicineBoxOutlined /> },
  { key: "prenotazioni", label: "Prenotazioni", path: "/prenotazioni", icona: <CalendarOutlined /> },
  { key: "prescrizioni", label: "Prescrizioni", path: "/prescrizioni", icona: <FileTextOutlined /> },
  { key: "referti", label: "Referti", path: "/referti", icona: <FilePdfOutlined /> },
  { key: "fatture", label: "Fatture", path: "/fatture", icona: <EuroCircleOutlined /> },
  { key: "medici", label: "Medici", path: "/medici", icona: <TeamOutlined />, ruoli: ["Amministratore"] },
  { key: "turni", label: "Turni", path: "/turni", icona: <ScheduleOutlined />, ruoli: ["Amministratore", "Medico"] },
];

export function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const vociVisibili = useMemo(
    () =>
      VOCI_MENU.filter((voce) => !voce.ruoli || voce.ruoli.some((ruolo) => user?.ruoli.includes(ruolo))),
    [user]
  );

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Sider breakpoint="lg" style={{ background: palette.primary }}>
        <div
          style={{
            color: "#fff",
            textAlign: "center",
            padding: "16px",
            fontSize: "18px",
            fontWeight: 600
          }}
        >
          MediCore
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          style={{ background: palette.primary, borderRight: "none" }}
          items={vociVisibili.map((voce) => ({
            key: voce.path,
            icon: voce.icona,
            label: <Link to={voce.path}>{voce.label}</Link>,
          }))}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            display: "flex",
            justifyContent: "flex-end",
            alignItems: "center",
            gap: 16,
            background: "#fff",
            boxShadow: "none",
            borderBottom: `3px solid ${palette.primary}`,
          }}
        >
          <Typography.Text style={{ color: palette.primary }}>{user?.email}</Typography.Text>
          <Button type="primary" onClick={handleLogout}>Logout</Button>
        </Header>
        <Content style={{ padding: 24 }}>
          <Card>
            <Outlet />
          </Card>
        </Content>
      </Layout>
    </Layout>
  );
}
