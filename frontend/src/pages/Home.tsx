import { Typography } from "antd";
import { useAuth } from "../auth/AuthContext";

export function Home() {
  const { user } = useAuth();

  return (
    <div>
      <Typography.Title level={2}>Benvenuto, {user?.email}</Typography.Title>
      <Typography.Paragraph>
        Seleziona una voce dal menu per iniziare.
      </Typography.Paragraph>
    </div>
  );
}
