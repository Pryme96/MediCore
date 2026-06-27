import { useState } from "react";
import { Form, Input, Button, Card, Typography, Alert, DatePicker } from "antd";
import { Link, useNavigate } from "react-router-dom";
import dayjs, { type Dayjs } from "dayjs";
import { register } from "../api/auth";
import { getErrorMessage } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import type { RegisterRequest } from "../types/auth";

type RegisterFormValues = Omit<RegisterRequest, "dataNascita"> & { dataNascita: Dayjs };

export function Register() {
  const [errore, setErrore] = useState<string | null>(null);
  const [caricamento, setCaricamento] = useState(false);
  const { loginWithToken } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (values: RegisterFormValues) => {
    setErrore(null);
    setCaricamento(true);
    try {
      const response = await register({
        ...values,
        dataNascita: values.dataNascita.format("YYYY-MM-DD"),
      });
      loginWithToken(response.token);
      navigate("/");
    } catch (error) {
      setErrore(getErrorMessage(error, "Registrazione non riuscita: verifica i dati inseriti."));
    } finally {
      setCaricamento(false);
    }
  };

  return (
    <div style={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", width: "100%" }}>
      <Card style={{ width: 420 }}>
        <Typography.Title level={3} style={{ textAlign: "center" }}>
          Registrazione Paziente
        </Typography.Title>
        {errore && <Alert type="error" title={errore} style={{ marginBottom: 16 }} />}
        <Form layout="vertical" onFinish={handleSubmit} initialValues={{ dataNascita: dayjs() }}>
          <Form.Item name="nome" label="Nome" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="cognome" label="Cognome" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="codiceFiscale" label="Codice Fiscale" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="dataNascita" label="Data di nascita" rules={[{ required: true }]}>
            <DatePicker style={{ width: "100%" }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item name="telefono" label="Telefono" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: "email" }]}>
            <Input />
          </Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true, min: 8 }]}>
            <Input.Password />
          </Form.Item>
          <Button type="primary" htmlType="submit" block loading={caricamento}>
            Registrati
          </Button>
        </Form>
        <Typography.Paragraph style={{ textAlign: "center", marginTop: 16 }}>
          Hai già un account? <Link to="/login">Accedi</Link>
        </Typography.Paragraph>
      </Card>
    </div>
  );
}
