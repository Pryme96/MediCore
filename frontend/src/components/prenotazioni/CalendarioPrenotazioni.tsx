import { useMemo } from "react";
import { Badge, Calendar, Tooltip, Typography } from "antd";
import dayjs, { type Dayjs } from "dayjs";
import {
  ETICHETTE_STATO_PRENOTAZIONE,
  StatoPrenotazione,
  type Prenotazione,
} from "../../types/prenotazioni";
import { ETICHETTE_REGIME } from "../../types/servizi";

const COLORE_STATO_BADGE: Record<StatoPrenotazione, string> = {
  [StatoPrenotazione.Confermata]: "green",
  [StatoPrenotazione.Annullata]: "default",
  [StatoPrenotazione.Completata]: "blue",
  [StatoPrenotazione.NonPresentato]: "red",
};

const MAX_VISIBILI = 3;

interface CalendarioPrenotazioniProps {
  prenotazioni: Prenotazione[];
  // Mostra il medico nel tooltip (vista Amministratore); nell'agenda del Medico è superfluo.
  mostraMedico?: boolean;
}

const chiaveGiorno = (data: string | Dayjs) => dayjs(data).format("YYYY-MM-DD");

export function CalendarioPrenotazioni({ prenotazioni, mostraMedico = false }: CalendarioPrenotazioniProps) {
  const perGiorno = useMemo(() => {
    const mappa = new Map<string, Prenotazione[]>();
    prenotazioni.forEach((p) => {
      const chiave = chiaveGiorno(p.dataOraInizio);
      const lista = mappa.get(chiave) ?? [];
      lista.push(p);
      mappa.set(chiave, lista);
    });
    for (const lista of mappa.values()) {
      lista.sort((a, b) => dayjs(a.dataOraInizio).valueOf() - dayjs(b.dataOraInizio).valueOf());
    }
    return mappa;
  }, [prenotazioni]);

  return (
    <Calendar
      cellRender={(data, info) => {
        if (info.type !== "date") {
          return info.originNode;
        }
        const items = perGiorno.get(chiaveGiorno(data)) ?? [];
        if (items.length === 0) {
          return null;
        }
        return (
          <ul style={{ listStyle: "none", margin: 0, padding: 0, fontSize: 12 }}>
            {items.slice(0, MAX_VISIBILI).map((p) => (
              <li
                key={p.id}
                style={{ overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis" }}
              >
                <Tooltip
                  title={`${dayjs(p.dataOraInizio).format("HH:mm")} · ${p.pazienteNomeCompleto} — ${p.prestazioneNome}${mostraMedico ? ` (${p.medicoNomeCompleto})` : ""} · ${ETICHETTE_STATO_PRENOTAZIONE[p.stato]} · ${ETICHETTE_REGIME[p.regime]}`}
                >
                  <span>
                    <Badge color={COLORE_STATO_BADGE[p.stato]} /> {dayjs(p.dataOraInizio).format("HH:mm")}{" "}
                    {p.pazienteNomeCompleto}
                  </span>
                </Tooltip>
              </li>
            ))}
            {items.length > MAX_VISIBILI && (
              <li>
                <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                  +{items.length - MAX_VISIBILI} altre
                </Typography.Text>
              </li>
            )}
          </ul>
        );
      }}
    />
  );
}
