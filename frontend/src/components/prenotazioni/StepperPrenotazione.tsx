import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Input,
  Radio,
  Select,
  Space,
  Spin,
  Steps,
  Tooltip,
  Typography,
  message,
} from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { getErrorMessage } from "../../api/client";
import { palette } from "../../theme/colors";
import { getServizi, getPrestazioniPerServizio, getTariffePerPrestazione } from "../../api/servizi";
import { getPazienti } from "../../api/pazienti";
import { createPrenotazione, getSlotPerPrestazione } from "../../api/prenotazioni";
import { ETICHETTE_REGIME, type Prestazione, type Regime, type Servizio, type Tariffa } from "../../types/servizi";
import type { Slot } from "../../types/prenotazioni";
import type { Paziente } from "../../types/pazienti";
import { CalendarioSlot } from "./CalendarioSlot";

interface StepperPrenotazioneProps {
  // In modalità operatore (Amministratore/Medico) si prenota per un paziente scelto da una lista.
  modalitaOperatore?: boolean;
  onCompletato: () => void;
  onAnnulla: () => void;
}

type ChiavePasso = "paziente" | "servizio" | "prestazione" | "slot" | "regime" | "riepilogo";

const TITOLI_PASSO: Record<ChiavePasso, string> = {
  paziente: "Paziente",
  servizio: "Servizio",
  prestazione: "Prestazione",
  slot: "Data e ora",
  regime: "Regime",
  riepilogo: "Riepilogo",
};

export function StepperPrenotazione({
  modalitaOperatore = false,
  onCompletato,
  onAnnulla,
}: StepperPrenotazioneProps) {
  const chiavi: ChiavePasso[] = modalitaOperatore
    ? ["paziente", "servizio", "prestazione", "slot", "regime", "riepilogo"]
    : ["servizio", "prestazione", "slot", "regime", "riepilogo"];

  const [passo, setPasso] = useState(0);
  const [errore, setErrore] = useState("");

  const [pazienti, setPazienti] = useState<Paziente[]>([]);
  const [pazienteId, setPazienteId] = useState<string | null>(null);

  const [servizi, setServizi] = useState<Servizio[]>([]);
  const [servizioId, setServizioId] = useState<string | null>(null);

  const [prestazioni, setPrestazioni] = useState<Prestazione[]>([]);
  const [prestazione, setPrestazione] = useState<Prestazione | null>(null);
  const [caricamentoPrestazioni, setCaricamentoPrestazioni] = useState(false);

  const [slots, setSlots] = useState<Slot[]>([]);
  const [tariffe, setTariffe] = useState<Tariffa[]>([]);
  const [caricamentoSlot, setCaricamentoSlot] = useState(false);
  const [slot, setSlot] = useState<Slot | null>(null);

  const [regime, setRegime] = useState<Regime | null>(null);
  const [note, setNote] = useState("");

  const [invio, setInvio] = useState(false);

  useEffect(() => {
    getServizi()
      .then(setServizi)
      .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare i servizi.")));
    if (modalitaOperatore) {
      getPazienti()
        .then(setPazienti)
        .catch((error) => setErrore(getErrorMessage(error, "Impossibile caricare i pazienti.")));
    }
  }, [modalitaOperatore]);

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
    if (modalitaOperatore && !pazienteId) return;
    setInvio(true);
    setErrore("");
    try {
      await createPrenotazione({
        slotId: slot.id,
        regime,
        note: note.trim() || undefined,
        pazienteId: modalitaOperatore ? pazienteId! : undefined,
      });
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

  const chiave = chiavi[passo];
  const puoAvanzare =
    (chiave === "paziente" && pazienteId !== null) ||
    (chiave === "servizio" && servizioId !== null) ||
    (chiave === "prestazione" && prestazione !== null) ||
    (chiave === "slot" && slot !== null) ||
    (chiave === "regime" && regime !== null);

  const tariffaScelta = tariffe.find((t) => t.regime === regime);
  const servizioScelto = servizi.find((s) => s.id === servizioId);
  const pazienteScelto = pazienti.find((p) => p.id === pazienteId);

  return (
    <div>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {modalitaOperatore ? "Prenota per un paziente" : "Prenota una visita"}
        </Typography.Title>
        <Tooltip title="Torna alle prenotazioni">
          <Button icon={<ArrowLeftOutlined />} onClick={onAnnulla} />
        </Tooltip>
      </Space>

      <Steps current={passo} items={chiavi.map((c) => ({ title: TITOLI_PASSO[c] }))} style={{ marginBottom: 24 }} />

      {errore && <Alert type="error" message={errore} style={{ marginBottom: 16 }} closable onClose={() => setErrore("")} />}

      {chiave === "paziente" && (
        <div>
          <Typography.Text>Seleziona il paziente</Typography.Text>
          <Select
            showSearch
            placeholder="Cerca per cognome, nome o codice fiscale"
            style={{ width: "100%", maxWidth: 520, marginTop: 8, display: "block" }}
            value={pazienteId}
            onChange={(value) => setPazienteId(value)}
            filterOption={(input, option) =>
              (option?.label ?? "").toLowerCase().includes(input.toLowerCase())
            }
            options={pazienti.map((p) => ({
              value: p.id,
              label: `${p.cognome} ${p.nome} — ${p.codiceFiscale}`,
            }))}
          />
        </div>
      )}

      {chiave === "servizio" && (
        <div style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
          {servizi.map((s) => (
            <Card
              key={s.id}
              hoverable
              onClick={() => selezionaServizio(s.id)}
              style={{ width: 240, outline: servizioId === s.id ? `2px solid ${palette.primary}` : "none" }}
            >
              <Card.Meta title={s.nome} description={s.descrizione} />
            </Card>
          ))}
        </div>
      )}

      {chiave === "prestazione" &&
        (caricamentoPrestazioni ? (
          <Spin />
        ) : (
          <div style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
            {prestazioni.map((p) => (
              <Card
                key={p.id}
                hoverable
                onClick={() => selezionaPrestazione(p)}
                style={{ width: 260, outline: prestazione?.id === p.id ? `2px solid ${palette.primary}` : "none" }}
              >
                <Card.Meta title={p.nome} description={`${p.descrizione} — ${p.durataMinuti} min`} />
              </Card>
            ))}
          </div>
        ))}

      {chiave === "slot" &&
        (caricamentoSlot ? <Spin /> : <CalendarioSlot slots={slots} slotSelezionato={slot} onSelect={setSlot} />)}

      {chiave === "regime" && (
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

      {chiave === "riepilogo" && slot && (
        <Descriptions bordered column={1}>
          {modalitaOperatore && (
            <Descriptions.Item label="Paziente">
              {pazienteScelto && `${pazienteScelto.cognome} ${pazienteScelto.nome} — ${pazienteScelto.codiceFiscale}`}
            </Descriptions.Item>
          )}
          <Descriptions.Item label="Servizio">{servizioScelto?.nome}</Descriptions.Item>
          <Descriptions.Item label="Prestazione">{prestazione?.nome}</Descriptions.Item>
          <Descriptions.Item label="Medico">{slot.medicoNomeCompleto}</Descriptions.Item>
          <Descriptions.Item label="Data e ora">
            {dayjs(slot.dataOraInizio).format("DD/MM/YYYY HH:mm")} – {dayjs(slot.dataOraFine).format("HH:mm")}
          </Descriptions.Item>
          <Descriptions.Item label="Regime">
            {regime !== null && ETICHETTE_REGIME[regime]}
            {tariffaScelta && ` — € ${tariffaScelta.prezzo.toFixed(2)}`}
          </Descriptions.Item>
          <Descriptions.Item label="Note">{note.trim() || "—"}</Descriptions.Item>
        </Descriptions>
      )}

      <Space style={{ marginTop: 24 }}>
        {passo > 0 && <Button onClick={() => setPasso((p) => p - 1)}>Indietro</Button>}
        {passo < chiavi.length - 1 && (
          <Button type="primary" disabled={!puoAvanzare} onClick={() => setPasso((p) => p + 1)}>
            Avanti
          </Button>
        )}
        {passo === chiavi.length - 1 && (
          <Button type="primary" loading={invio} onClick={conferma}>
            Conferma prenotazione
          </Button>
        )}
      </Space>
    </div>
  );
}
