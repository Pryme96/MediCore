import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Input,
  Radio,
  Space,
  Spin,
  Steps,
  Typography,
  message,
} from "antd";
import dayjs from "dayjs";
import { getErrorMessage } from "../../api/client";
import { palette } from "../../theme/colors";
import { getServizi, getPrestazioniPerServizio, getTariffePerPrestazione } from "../../api/servizi";
import { createPrenotazione, getSlotPerPrestazione } from "../../api/prenotazioni";
import { ETICHETTE_REGIME, type Prestazione, type Servizio, type Tariffa } from "../../types/servizi";
import type { Slot } from "../../types/prenotazioni";
import { CalendarioSlot } from "./CalendarioSlot";

interface StepperPrenotazioneProps {
  onCompletato: () => void;
  onAnnulla: () => void;
}

const PASSI = [
  { title: "Servizio" },
  { title: "Prestazione" },
  { title: "Data e ora" },
  { title: "Regime" },
  { title: "Riepilogo" },
];

export function StepperPrenotazione({ onCompletato, onAnnulla }: StepperPrenotazioneProps) {
  const [passo, setPasso] = useState(0);
  const [errore, setErrore] = useState("");

  const [servizi, setServizi] = useState<Servizio[]>([]);
  const [servizioId, setServizioId] = useState<string | null>(null);

  const [prestazioni, setPrestazioni] = useState<Prestazione[]>([]);
  const [prestazione, setPrestazione] = useState<Prestazione | null>(null);
  const [caricamentoPrestazioni, setCaricamentoPrestazioni] = useState(false);

  const [slots, setSlots] = useState<Slot[]>([]);
  const [tariffe, setTariffe] = useState<Tariffa[]>([]);
  const [caricamentoSlot, setCaricamentoSlot] = useState(false);
  const [slot, setSlot] = useState<Slot | null>(null);

  const [regime, setRegime] = useState<number | null>(null);
  const [note, setNote] = useState("");

  const [invio, setInvio] = useState(false);

  useEffect(() => {
    getServizi()
      .then(setServizi)
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare i servizi.")));
  }, []);

  const selezionaServizio = (id: string) => {
    setServizioId(id);
    setPrestazione(null);
    setPrestazioni([]);
    setSlots([]);
    setTariffe([]);
    setSlot(null);
    setRegime(null);
    setCaricamentoPrestazioni(true);
    getPrestazioniPerServizio(id)
      .then(setPrestazioni)
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare le prestazioni.")))
      .finally(() => setCaricamentoPrestazioni(false));
  };

  const selezionaPrestazione = (p: Prestazione) => {
    setPrestazione(p);
    setSlot(null);
    setRegime(null);
    setCaricamentoSlot(true);
    setErrore("");
    Promise.all([getSlotPerPrestazione(p.id), getTariffePerPrestazione(p.id)])
      .then(([listaSlot, listaTariffe]) => {
        setSlots(listaSlot);
        setTariffe(listaTariffe);
      })
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare le disponibilità.")))
      .finally(() => setCaricamentoSlot(false));
  };

  const conferma = async () => {
    if (!slot || regime === null) return;
    setInvio(true);
    setErrore("");
    try {
      await createPrenotazione({ slotId: slot.id, regime, note: note.trim() || undefined });
      message.success("Prenotazione effettuata.");
      onCompletato();
    } catch (error) {
      setErrore(
        getErrorMessage(error, "Impossibile completare la prenotazione: lo slot potrebbe non essere più disponibile.")
      );
    } finally {
      setInvio(false);
    }
  };

  const puoAvanzare =
    (passo === 0 && servizioId !== null) ||
    (passo === 1 && prestazione !== null) ||
    (passo === 2 && slot !== null) ||
    (passo === 3 && regime !== null);

  const tariffaScelta = tariffe.find((t) => t.regime === regime);
  const servizioScelto = servizi.find((s) => s.id === servizioId);

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          Prenota una visita
        </Typography.Title>
        <Button onClick={onAnnulla}>Torna alle prenotazioni</Button>
      </Space>

      <Steps current={passo} items={PASSI} style={{ marginBottom: 24 }} />

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} closable onClose={() => setErrore("")} />}

      {/* Passo 1: Servizio */}
      {passo === 0 && (
        <Space wrap>
          {servizi.map((s) => (
            <Card
              key={s.id}
              hoverable
              onClick={() => selezionaServizio(s.id)}
              style={{
                width: 240,
                outline: servizioId === s.id ? `2px solid ${palette.primary}` : "none",
              }}
            >
              <Card.Meta title={s.nome} description={s.descrizione} />
            </Card>
          ))}
        </Space>
      )}

      {/* Passo 2: Prestazione */}
      {passo === 1 &&
        (caricamentoPrestazioni ? (
          <Spin />
        ) : (
          <Space wrap>
            {prestazioni.map((p) => (
              <Card
                key={p.id}
                hoverable
                onClick={() => selezionaPrestazione(p)}
                style={{
                  width: 260,
                  outline: prestazione?.id === p.id ? `2px solid ${palette.primary}` : "none",
                }}
              >
                <Card.Meta title={p.nome} description={`${p.descrizione} — ${p.durataMinuti} min`} />
              </Card>
            ))}
          </Space>
        ))}

      {/* Passo 3: Data e ora */}
      {passo === 2 &&
        (caricamentoSlot ? (
          <Spin />
        ) : (
          <CalendarioSlot slots={slots} slotSelezionato={slot} onSelect={setSlot} />
        ))}

      {/* Passo 4: Regime + note */}
      {passo === 3 && (
        <div>
          {tariffe.length === 0 ? (
            <Alert
              type="warning"
              showIcon
              message="Nessuna tariffa configurata per questa prestazione: prenotazione non disponibile."
            />
          ) : (
            <Radio.Group
              value={regime}
              onChange={(e) => setRegime(e.target.value)}
              style={{ display: "flex", flexDirection: "column", gap: 8 }}
            >
              {tariffe.map((t) => (
                <Radio key={t.id} value={t.regime}>
                  {ETICHETTE_REGIME[t.regime]} — € {t.prezzo.toFixed(2)}
                </Radio>
              ))}
            </Radio.Group>
          )}
          <div style={{ marginTop: 16 }}>
            <Typography.Text>Note (opzionali)</Typography.Text>
            <Input.TextArea
              rows={3}
              value={note}
              onChange={(e) => setNote(e.target.value)}
              maxLength={500}
              style={{ marginTop: 4 }}
            />
          </div>
        </div>
      )}

      {/* Passo 5: Riepilogo */}
      {passo === 4 && slot && (
        <Descriptions bordered column={1}>
          <Descriptions.Item label="Servizio">{servizioScelto?.nome}</Descriptions.Item>
          <Descriptions.Item label="Prestazione">{prestazione?.nome}</Descriptions.Item>
          <Descriptions.Item label="Medico">{slot.medicoNomeCompleto}</Descriptions.Item>
          <Descriptions.Item label="Data e ora">
            {dayjs(slot.dataOraInizio).format("DD/MM/YYYY HH:mm")} –{" "}
            {dayjs(slot.dataOraFine).format("HH:mm")}
          </Descriptions.Item>
          <Descriptions.Item label="Regime">
            {regime !== null && ETICHETTE_REGIME[regime as Tariffa["regime"]]}
            {tariffaScelta && ` — € ${tariffaScelta.prezzo.toFixed(2)}`}
          </Descriptions.Item>
          <Descriptions.Item label="Note">{note.trim() || "—"}</Descriptions.Item>
        </Descriptions>
      )}

      <Space style={{ marginTop: 24 }}>
        {passo > 0 && <Button onClick={() => setPasso((p) => p - 1)}>Indietro</Button>}
        {passo < 4 && (
          <Button type="primary" disabled={!puoAvanzare} onClick={() => setPasso((p) => p + 1)}>
            Avanti
          </Button>
        )}
        {passo === 4 && (
          <Button type="primary" loading={invio} onClick={conferma}>
            Conferma prenotazione
          </Button>
        )}
      </Space>
    </div>
  );
}
