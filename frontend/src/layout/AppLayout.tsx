import { useMemo } from "react";
import { Layout, Menu, Button, Typography, Card, Avatar } from "antd";
import {
  MedicineBoxOutlined,
  CalendarOutlined,
  FileTextOutlined,
  FilePdfOutlined,
  EuroCircleOutlined,
  TeamOutlined,
  ScheduleOutlined,
  UserOutlined,
  LogoutOutlined,
  AppstoreOutlined,
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
  { key: "gestione-servizi", label: "Gestione Servizi", path: "/gestione-servizi", icona: <AppstoreOutlined />, ruoli: ["Amministratore"] },
  { key: "servizi", label: "Servizi", path: "/servizi", icona: <MedicineBoxOutlined />, ruoli: ["Paziente", "Medico"] },
  { key: "prenotazioni", label: "Prenotazioni", path: "/prenotazioni", icona: <CalendarOutlined />, ruoli: ["Paziente"] },
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
            height: 64,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            background: "#fff",
          }}
        >
          <Link to="/" style={{ display: "flex", alignItems: "center" }}>
            <img src="/logo.png" alt="MediCore" style={{ display: "block", maxHeight: 52, maxWidth: "100%" }} />
          </Link>
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
            boxShadow: "0 2px 8px rgba(0, 0, 0, 0.06)",
          }}
        >
          <Avatar icon={<UserOutlined />} style={{ background: palette.primary }} />
          <Typography.Text style={{ color: palette.primary }}>{user?.email}</Typography.Text>
          <Button type="primary" icon={<LogoutOutlined />} onClick={handleLogout}>Logout</Button>
        </Header>
        <Content style={{ padding: 24 }}>
            <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
