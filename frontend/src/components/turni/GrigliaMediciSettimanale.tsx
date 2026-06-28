import { Empty, Tooltip } from "antd";
import { palette } from "../../theme/colors";
import { ETICHETTE_GIORNO, GiornoSettimana, type Turno } from "../../types/turni";
import type { Medico } from "../../types/medici";

interface GrigliaMediciSettimanaleProps {
  medici: Medico[];
  turni: Turno[];
}

const GIORNI = Object.values(GiornoSettimana);
const ORA_APERTURA = 8;
const ORA_CHIUSURA = 20;
const ORE = ORA_CHIUSURA - ORA_APERTURA; // 12 celle orarie per giorno
const LARGHEZZA_ETICHETTA = 150;
const ALTEZZA_RIGA = 30;
const COLONNE_TOTALI = GIORNI.length * ORE;

const minutiDa = (ora: string): number => {
  const [h, m] = ora.split(":").map(Number);
  return h * 60 + m;
};

const bordoGiorno = `2px solid ${palette.accentSoft}`;
const bordoOra = `1px solid ${palette.backgroundTint}`;

export function GrigliaMediciSettimanale({ medici, turni }: GrigliaMediciSettimanaleProps) {
  if (medici.length === 0) {
    return <Empty description="Nessun medico da mostrare." />;
  }

  const rigaDiMedico = new Map(medici.map((medico, indice) => [medico.id, indice]));

  return (
    <div style={{ overflowX: "auto" }}>
      <div
        style={{
          display: "grid",
          gridTemplateColumns: `${LARGHEZZA_ETICHETTA}px repeat(${COLONNE_TOTALI}, 1fr)`,
          gridTemplateRows: `28px 18px repeat(${medici.length}, ${ALTEZZA_RIGA}px)`,
          border: `1px solid ${palette.backgroundTint}`,
          borderRadius: 8,
          overflow: "hidden",
          fontSize: 12,
        }}
      >
        {/* Angolo in alto a sinistra (copre le due righe di intestazione) */}
        <div style={{ gridColumn: 1, gridRow: "1 / 3", background: palette.backgroundTint }} />

        {/* Intestazione giorni */}
        {GIORNI.map((giorno, d) => (
          <div
            key={`giorno-${giorno}`}
            style={{
              gridColumn: `${2 + d * ORE} / ${2 + (d + 1) * ORE}`,
              gridRow: 1,
              background: palette.backgroundTint,
              color: palette.primaryDark,
              fontWeight: 600,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              borderLeft: bordoGiorno,
            }}
          >
            {ETICHETTE_GIORNO[giorno]}
          </div>
        ))}

        {/* Tacche orarie sotto ogni giorno */}
        {GIORNI.map((giorno, d) =>
          Array.from({ length: ORE }, (_, h) => (
            <div
              key={`ora-${giorno}-${h}`}
              style={{
                gridColumn: 2 + d * ORE + h,
                gridRow: 2,
                background: palette.backgroundTint,
                color: "#7a7a7a",
                fontSize: 9,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                borderLeft: h === 0 ? bordoGiorno : bordoOra,
              }}
            >
              {ORA_APERTURA + h}
            </div>
          ))
        )}

        {/* Etichette medici (colonna sinistra) */}
        {medici.map((medico, i) => (
          <div
            key={`medico-${medico.id}`}
            style={{
              gridColumn: 1,
              gridRow: i + 3,
              display: "flex",
              alignItems: "center",
              padding: "0 8px",
              borderTop: bordoOra,
              fontWeight: 500,
              whiteSpace: "nowrap",
              overflow: "hidden",
              textOverflow: "ellipsis",
            }}
          >
            {`${medico.cognome} ${medico.nome}`}
          </div>
        ))}

        {/* Sfondo per ogni medico/giorno con le linee orarie */}
        {medici.map((medico, i) =>
          GIORNI.map((giorno, d) => (
            <div
              key={`bg-${medico.id}-${giorno}`}
              style={{
                gridColumn: `${2 + d * ORE} / ${2 + (d + 1) * ORE}`,
                gridRow: i + 3,
                borderTop: bordoOra,
                borderLeft: bordoGiorno,
                backgroundImage: `repeating-linear-gradient(to right, transparent 0, transparent calc(100% / ${ORE} - 1px), ${palette.backgroundTint} calc(100% / ${ORE} - 1px), ${palette.backgroundTint} calc(100% / ${ORE}))`,
              }}
            />
          ))
        )}

        {/* Blocchi turno: per ogni medico, coprono le ore del proprio turno */}
        {turni.map((turno) => {
          const riga = rigaDiMedico.get(turno.medicoId);
          if (riga === undefined) return null;

          const d = GIORNI.indexOf(turno.giornoSettimana);
          const inizioOra = Math.max(0, Math.floor(minutiDa(turno.oraInizio) / 60) - ORA_APERTURA);
          const fineOra = Math.min(ORE, Math.ceil(minutiDa(turno.oraFine) / 60) - ORA_APERTURA);
          if (fineOra <= inizioOra) return null;

          return (
            <Tooltip
              key={turno.id}
              title={`${turno.prestazioneNome} (${turno.oraInizio.slice(0, 5)}–${turno.oraFine.slice(0, 5)})`}
            >
              <div
                style={{
                  gridColumn: `${2 + d * ORE + inizioOra} / ${2 + d * ORE + fineOra}`,
                  gridRow: riga + 3,
                  background: palette.primary,
                  color: "#fff",
                  borderRadius: 5,
                  margin: 3,
                  padding: "0 4px",
                  display: "flex",
                  alignItems: "center",
                  overflow: "hidden",
                  whiteSpace: "nowrap",
                  textOverflow: "ellipsis",
                  cursor: "default",
                }}
              >
                <span
                  style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}
                >
                  {turno.prestazioneNome}
                </span>
              </div>
            </Tooltip>
          );
        })}
      </div>
    </div>
  );
}
