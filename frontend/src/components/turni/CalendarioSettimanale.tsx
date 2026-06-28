import { Empty, Tooltip } from "antd";
import { palette } from "../../theme/colors";
import { ETICHETTE_GIORNO, GiornoSettimana, type Turno } from "../../types/turni";

interface CalendarioSettimanaleProps {
  turni: Turno[];
  // Mostra il nome del medico nel blocco (vista Amministratore); nascosto nella vista Medico.
  mostraMedico?: boolean;
}

const GIORNI = Object.values(GiornoSettimana);
const ALTEZZA_UNITA = 24; // px per ogni mezz'ora
const ORA_MIN_DEFAULT = 8;
const ORA_MAX_DEFAULT = 20;

const minutiDa = (ora: string): number => {
  const [h, m] = ora.split(":").map(Number);
  return h * 60 + m;
};

interface Posizione {
  corsia: number;
  totale: number;
}

// Per ogni giorno, assegna i turni sovrapposti a corsie affiancate (come un calendario):
// i turni che si sovrappongono in un cluster si dividono la larghezza della colonna.
const calcolaPosizioni = (turni: Turno[]): Map<string, Posizione> => {
  const posizioni = new Map<string, Posizione>();

  const perGiorno = new Map<number, Turno[]>();
  for (const turno of turni) {
    const lista = perGiorno.get(turno.giornoSettimana) ?? [];
    lista.push(turno);
    perGiorno.set(turno.giornoSettimana, lista);
  }

  for (const lista of perGiorno.values()) {
    const ordinati = [...lista].sort((a, b) => minutiDa(a.oraInizio) - minutiDa(b.oraInizio));

    let cluster: Turno[] = [];
    let fineMassimaCluster = -1;

    const chiudiCluster = () => {
      if (cluster.length === 0) return;
      const fineCorsie: number[] = [];
      const corsiaDi = new Map<string, number>();
      for (const turno of cluster) {
        const inizio = minutiDa(turno.oraInizio);
        let corsia = fineCorsie.findIndex((fine) => fine <= inizio);
        if (corsia === -1) {
          corsia = fineCorsie.length;
          fineCorsie.push(minutiDa(turno.oraFine));
        } else {
          fineCorsie[corsia] = minutiDa(turno.oraFine);
        }
        corsiaDi.set(turno.id, corsia);
      }
      const totale = fineCorsie.length;
      for (const turno of cluster) {
        posizioni.set(turno.id, { corsia: corsiaDi.get(turno.id)!, totale });
      }
      cluster = [];
      fineMassimaCluster = -1;
    };

    for (const turno of ordinati) {
      if (cluster.length > 0 && minutiDa(turno.oraInizio) >= fineMassimaCluster) {
        chiudiCluster();
      }
      cluster.push(turno);
      fineMassimaCluster = Math.max(fineMassimaCluster, minutiDa(turno.oraFine));
    }
    chiudiCluster();
  }

  return posizioni;
};

export function CalendarioSettimanale({ turni, mostraMedico = false }: CalendarioSettimanaleProps) {
  if (turni.length === 0) {
    return <Empty description="Nessun turno da mostrare nel calendario." />;
  }

  // Finestra oraria visibile: dal turno più mattutino al più serale, arrotondata all'ora.
  const oraMin = Math.min(ORA_MIN_DEFAULT, ...turni.map((t) => Math.floor(minutiDa(t.oraInizio) / 60)));
  const oraMax = Math.max(ORA_MAX_DEFAULT, ...turni.map((t) => Math.ceil(minutiDa(t.oraFine) / 60)));
  const unitaTotali = (oraMax - oraMin) * 2;

  // Una unità di griglia = 30 minuti. Riga 1 = intestazione giorni.
  const unitaDa = (ora: string) => (minutiDa(ora) - oraMin * 60) / 30;

  const ore = Array.from({ length: oraMax - oraMin + 1 }, (_, i) => oraMin + i);
  const posizioni = calcolaPosizioni(turni);

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "64px repeat(7, 1fr)",
        gridTemplateRows: `32px repeat(${unitaTotali}, ${ALTEZZA_UNITA}px)`,
        border: `1px solid ${palette.backgroundTint}`,
        borderRadius: 8,
        overflow: "hidden",
        fontSize: 12,
      }}
    >
      {/* Angolo in alto a sinistra */}
      <div style={{ gridColumn: 1, gridRow: 1, background: palette.backgroundTint }} />

      {/* Intestazione giorni */}
      {GIORNI.map((giorno, indice) => (
        <div
          key={`head-${giorno}`}
          style={{
            gridColumn: indice + 2,
            gridRow: 1,
            background: palette.backgroundTint,
            color: palette.primaryDark,
            fontWeight: 600,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          {ETICHETTE_GIORNO[giorno]}
        </div>
      ))}

      {/* Etichette orarie (colonna sinistra) */}
      {ore.map((ora, indice) => (
        <div
          key={`ora-${ora}`}
          style={{
            gridColumn: 1,
            gridRow: indice * 2 + 2,
            borderTop: indice === 0 ? "none" : `1px solid ${palette.backgroundTint}`,
            color: "#888",
            paddingRight: 6,
            textAlign: "right",
          }}
        >
          {String(ora).padStart(2, "0")}:00
        </div>
      ))}

      {/* Celle di sfondo (una per ora per giorno) per le linee della griglia */}
      {GIORNI.map((giorno, colonna) =>
        ore.slice(0, -1).map((ora, riga) => (
          <div
            key={`bg-${giorno}-${ora}`}
            style={{
              gridColumn: colonna + 2,
              gridRow: riga * 2 + 2,
              gridRowEnd: "span 2",
              borderTop: riga === 0 ? "none" : `1px solid ${palette.backgroundTint}`,
              borderLeft: `1px solid ${palette.backgroundTint}`,
            }}
          />
        ))
      )}

      {/* Blocchi turno (affiancati in corsie se sovrapposti) */}
      {turni.map((turno) => {
        const colonna = GIORNI.indexOf(turno.giornoSettimana) + 2;
        const inizio = unitaDa(turno.oraInizio);
        const fine = unitaDa(turno.oraFine);
        const { corsia, totale } = posizioni.get(turno.id) ?? { corsia: 0, totale: 1 };
        const larghezza = 100 / totale;
        // Il dettaglio mostrato dipende dallo spazio: con molte corsie il blocco è stretto,
        // quindi si riduce a sola barra colorata (i dettagli restano nel tooltip).
        const dettaglio = totale === 1 ? "completo" : totale <= 3 ? "titolo" : "nessuno";
        const testoRidotto: React.CSSProperties = {
          whiteSpace: "nowrap",
          overflow: "hidden",
          textOverflow: "ellipsis",
        };
        return (
          <Tooltip
            key={turno.id}
            title={`${turno.prestazioneNome}${mostraMedico ? ` — ${turno.medicoNomeCompleto}` : ""} (${turno.oraInizio.slice(0, 5)}–${turno.oraFine.slice(0, 5)})`}
          >
            <div
              style={{
                gridColumn: colonna,
                gridRow: `${inizio + 2} / ${fine + 2}`,
                justifySelf: "stretch",
                width: `calc(${larghezza}% - 4px)`,
                marginLeft: `calc(${corsia * larghezza}% + 2px)`,
                marginTop: 1,
                marginBottom: 1,
                boxSizing: "border-box",
                background: palette.primary,
                color: "#fff",
                borderRadius: 6,
                padding: dettaglio === "nessuno" ? 0 : "2px 6px",
                overflow: "hidden",
                lineHeight: 1.3,
                cursor: "default",
              }}
            >
              {dettaglio === "completo" && (
                <>
                  <div style={{ fontWeight: 600, ...testoRidotto }}>{turno.prestazioneNome}</div>
                  {mostraMedico && <div style={testoRidotto}>{turno.medicoNomeCompleto}</div>}
                  <div style={testoRidotto}>
                    {turno.oraInizio.slice(0, 5)}–{turno.oraFine.slice(0, 5)}
                  </div>
                </>
              )}
              {dettaglio === "titolo" && (
                <div style={{ fontWeight: 600, ...testoRidotto }}>
                  {mostraMedico ? turno.medicoNomeCompleto : turno.prestazioneNome}
                </div>
              )}
            </div>
          </Tooltip>
        );
      })}
    </div>
  );
}
