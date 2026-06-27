import { useState } from "react";
import { Form, Input, Button, Card, Typography, Alert } from "antd";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/auth";
import { getErrorMessage } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { palette } from "../theme/colors";
import type { LoginRequest } from "../types/auth";

export function Login() {
  const [errore, setErrore] = useState<string | null>(null);
  const [caricamento, setCaricamento] = useState(false);
  const { loginWithToken } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (values: LoginRequest) => {
    setErrore(null);
    setCaricamento(true);
    try {
      const response = await login(values);
      loginWithToken(response.token);
      navigate("/");
    } catch (error) {
      setErrore(getErrorMessage(error, "Credenziali non valide."));
    } finally {
      setCaricamento(false);
    }
  };

  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        minHeight: "100vh",
        width: "100%",
        background: `linear-gradient(180deg, ${palette.primaryDark}, ${palette.backgroundTint})`,
      }}
    >
      <Card style={{ width: 360 }}>
        <div style={{ display: "flex", justifyContent: "center", marginBottom: 16 }}>
          <img src="/logo.png" alt="MediCore" style={{ maxHeight: 64 }} />
        </div>
        <Typography.Title level={3} style={{ textAlign: "center" }}>
          Accedi a MediCore
        </Typography.Title>
        {errore && <Alert type="error" title={errore} style={{ marginBottom: 16 }} />}
        <Form layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: "email" }]}>
            <Input />
          </Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true }]}>
            <Input.Password />
          </Form.Item>
          <Button type="primary" htmlType="submit" block loading={caricamento}>
            Accedi
          </Button>
        </Form>
        <Typography.Paragraph style={{ textAlign: "center", marginTop: 16 }}>
          Non hai un account? <Link to="/register">Registrati</Link>
        </Typography.Paragraph>
      </Card>
    </div>
  );
}
