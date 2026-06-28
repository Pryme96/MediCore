import { useState } from "react";
import { Alert, Button, Card, Collapse, Input, Space, Typography } from "antd";
import { BulbOutlined } from "@ant-design/icons";
import { suggerisciPrescrizione } from "../../api/ai";
import { getErrorMessage } from "../../api/client";
import { Sesso, type SuggerimentoOpzione, type SuggerimentoResponse } from "../../types/ai";
import { TipoPrescrizione } from "../../types/prescrizioni";

interface Props {
  pazienteId?: string;
  tipo?: TipoPrescrizione;
  onApplica: (opzione: SuggerimentoOpzione) => void;
}

const ETICHETTE_SESSO: Record<Sesso, string> = {
  [Sesso.Maschile]: "M",
  [Sesso.Femminile]: "F",
};

export function PannelloAssistenteAi({ pazienteId, tipo, onApplica }: Props) {
  const [aperto, setAperto] = useState(false);
  const [contestoClinico, setContestoClinico] = useState("");
  const [allergie, setAllergie] = useState("");
  const [terapieInCorso, setTerapieInCorso] = useState("");
  const [caricamento, setCaricamento] = useState(false);
  const [errore, setErrore] = useState("");
  const [risposta, setRisposta] = useState<SuggerimentoResponse | null>(null);

  const handleSuggerisci = async () => {
    if (!pazienteId || !contestoClinico.trim()) return;
    setCaricamento(true);
    setErrore("");
    try {
      const risultato = await suggerisciPrescrizione({
        pazienteId,
        tipo: tipo ?? TipoPrescrizione.Farmacologica,
        contestoClinico,
        allergie: allergie.trim() ? allergie : undefined,
        terapieInCorso: terapieInCorso.trim() ? terapieInCorso : undefined,
      });
      setRisposta(risultato);
    } catch (error) {
      setErrore(getErrorMessage(error, "Impossibile generare suggerimenti."));
    } finally {
      setCaricamento(false);
    }
  };

  return (
    <Card
      size="small"
      style={{ marginBottom: 16, background: "#fafafa" }}
      title={
        <Space>
          <BulbOutlined />
          Assistenza alla redazione
        </Space>
      }
      extra={
        <Button size="small" type="link" onClick={() => setAperto((a) => !a)}>
          {aperto ? "Nascondi" : "Apri"}
        </Button>
      }
    >
      {aperto && (
        <>
          <Typography.Paragraph type="secondary" style={{ fontSize: 12, marginBottom: 8 }}>
            Verranno inviati: età, sesso e il contesto clinico/allergie/terapie indicati qui
            sotto. Nessun dato identificativo (nome, codice fiscale, contatti) viene trasmesso.
            Evita di scrivere dati identificativi nel testo libero.
          </Typography.Paragraph>

          {!pazienteId && (
            <Alert type="info" showIcon message="Seleziona prima un paziente." style={{ marginBottom: 8 }} />
          )}

          <Space direction="vertical" style={{ width: "100%" }}>
            <Input.TextArea
              rows={2}
              placeholder="Contesto clinico (diagnosi, quesito, sintomi)"
              value={contestoClinico}
              onChange={(e) => setContestoClinico(e.target.value)}
              disabled={!pazienteId}
            />
            <Input
              placeholder="Allergie note (facoltativo)"
              value={allergie}
              onChange={(e) => setAllergie(e.target.value)}
              disabled={!pazienteId}
            />
            <Input
              placeholder="Terapie in corso (facoltativo)"
              value={terapieInCorso}
              onChange={(e) => setTerapieInCorso(e.target.value)}
              disabled={!pazienteId}
            />
            <Button
              type="primary"
              ghost
              onClick={handleSuggerisci}
              loading={caricamento}
              disabled={!pazienteId || !contestoClinico.trim()}
            >
              Suggerisci
            </Button>
          </Space>

          {errore && <Alert type="error" message={errore} style={{ marginTop: 12 }} />}

          {risposta && (
            <div style={{ marginTop: 16 }}>
              {risposta.demo && (
                <Alert
                  type="warning"
                  showIcon
                  message="Suggerimenti dimostrativi: nessun servizio AI configurato."
                  style={{ marginBottom: 12 }}
                />
              )}

              <Collapse
                size="small"
                style={{ marginBottom: 12 }}
                items={[
                  {
                    key: "dati",
                    label: "Dati inviati all'assistente (de-identificati)",
                    children: (
                      <div style={{ fontSize: 12 }}>
                        <div>Età: {risposta.datiInviati.eta} anni</div>
                        <div>
                          Sesso:{" "}
                          {risposta.datiInviati.sesso ? ETICHETTE_SESSO[risposta.datiInviati.sesso] : "non specificato"}
                        </div>
                        <div>Contesto clinico: {risposta.datiInviati.contestoClinico}</div>
                        {risposta.datiInviati.allergie && <div>Allergie: {risposta.datiInviati.allergie}</div>}
                        {risposta.datiInviati.terapieInCorso && (
                          <div>Terapie in corso: {risposta.datiInviati.terapieInCorso}</div>
                        )}
                      </div>
                    ),
                  },
                ]}
              />

              {risposta.opzioni.length === 0 ? (
                <Alert type="info" message="Nessun suggerimento disponibile: procedi manualmente." />
              ) : (
                risposta.opzioni.map((opzione, indice) => (
                  <Card key={indice} size="small" style={{ marginBottom: 8 }}>
                    {opzione.diagnosiSuggerita && (
                      <div style={{ marginBottom: 4 }}>
                        <strong>Diagnosi:</strong> {opzione.diagnosiSuggerita}
                      </div>
                    )}
                    <ul style={{ margin: "4px 0 8px", paddingLeft: 20 }}>
                      {opzione.righe.map((riga, i) => (
                        <li key={i}>
                          {riga.farmaco} — {riga.posologia} (x{riga.quantita})
                        </li>
                      ))}
                    </ul>
                    {opzione.durataGiorni != null && (
                      <div style={{ marginBottom: 4 }}>
                        <strong>Durata:</strong> {opzione.durataGiorni} giorni
                      </div>
                    )}
                    {opzione.monitoraggio && (
                      <div style={{ marginBottom: 4 }}>
                        <strong>Monitoraggio:</strong> {opzione.monitoraggio}
                      </div>
                    )}
                    <div style={{ marginBottom: 4 }}>
                      <strong>Motivazione:</strong> {opzione.motivazione}
                    </div>
                    {opzione.avvertenze && (
                      <Alert type="warning" showIcon message={opzione.avvertenze} style={{ marginBottom: 8 }} />
                    )}
                    <Button size="small" onClick={() => onApplica(opzione)}>
                      Usa questa proposta
                    </Button>
                  </Card>
                ))
              )}
            </div>
          )}
        </>
      )}
    </Card>
  );
}
