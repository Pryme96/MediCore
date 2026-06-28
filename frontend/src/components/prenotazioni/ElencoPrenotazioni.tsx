import { useMemo, useState } from "react";
import {
  Button,
  DatePicker,
  Input,
  Popconfirm,
  Segmented,
  Select,
  Space,
  Table,
  Tag,
  Typography,
} from "antd";
import { ReloadOutlined } from "@ant-design/icons";
import dayjs, { type Dayjs } from "dayjs";
import {
  ETICHETTE_STATO_PRENOTAZIONE,
  StatoPrenotazione,
  type Prenotazione,
} from "../../types/prenotazioni";
import { ETICHETTE_REGIME, Regime } from "../../types/servizi";
import { CalendarioPrenotazioni } from "./CalendarioPrenotazioni";

const COLORE_STATO: Record<StatoPrenotazione, string> = {
  [StatoPrenotazione.Confermata]: "green",
  [StatoPrenotazione.Annullata]: "default",
  [StatoPrenotazione.Completata]: "blue",
  [StatoPrenotazione.NonPresentato]: "red",
};

const OPZIONI_VISTA = [
  { label: "Tabella", value: "tabella" },
  { label: "Calendario", value: "calendario" },
];

interface ElencoPrenotazioniProps {
  prenotazioni: Prenotazione[];
  onAnnulla: (p: Prenotazione) => void;
  // Mostra la colonna Medico e lo include nella ricerca (vista Amministratore).
  mostraMedico?: boolean;
  emptyText: string;
}

export function ElencoPrenotazioni({
  prenotazioni,
  onAnnulla,
  mostraMedico = false,
  emptyText,
}: ElencoPrenotazioniProps) {
  const [vista, setVista] = useState<"tabella" | "calendario">("tabella");
  const [ricerca, setRicerca] = useState("");
  const [statiFiltro, setStatiFiltro] = useState<number[]>([]);
  const [regimiFiltro, setRegimiFiltro] = useState<number[]>([]);
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null);

  const prenotazioniFiltrate = useMemo(() => {
    const testo = ricerca.trim().toLowerCase();
    const inizio = periodo?.[0].startOf("day");
    const fine = periodo?.[1].endOf("day");

    return prenotazioni.filter((p) => {
      if (testo) {
        const campo = `${p.pazienteNomeCompleto} ${p.prestazioneNome} ${mostraMedico ? p.medicoNomeCompleto : ""}`.toLowerCase();
        if (!campo.includes(testo)) return false;
      }
      if (statiFiltro.length > 0 && !statiFiltro.includes(p.stato)) return false;
      if (regimiFiltro.length > 0 && !regimiFiltro.includes(p.regime)) return false;
      if (inizio && fine) {
        const data = dayjs(p.dataOraInizio);
        if (data.isBefore(inizio) || data.isAfter(fine)) return false;
      }
      return true;
    });
  }, [prenotazioni, ricerca, statiFiltro, regimiFiltro, periodo, mostraMedico]);

  const filtriAttivi =
    ricerca.trim() !== "" || statiFiltro.length > 0 || regimiFiltro.length > 0 || periodo !== null;

  const resetFiltri = () => {
    setRicerca("");
    setStatiFiltro([]);
    setRegimiFiltro([]);
    setPeriodo(null);
  };

  return (
    <div>
      <Space wrap style={{ width: "100%", justifyContent: "space-between", marginBottom: 12 }}>
        <Space wrap>
          <Input.Search
            allowClear
            placeholder={mostraMedico ? "Cerca paziente, medico o prestazione" : "Cerca paziente o prestazione"}
            style={{ width: 280 }}
            value={ricerca}
            onChange={(e) => setRicerca(e.target.value)}
          />
          <Select
            mode="multiple"
            allowClear
            placeholder="Stato"
            style={{ minWidth: 200 }}
            value={statiFiltro}
            onChange={setStatiFiltro}
            options={Object.values(StatoPrenotazione).map((s) => ({
              value: s,
              label: ETICHETTE_STATO_PRENOTAZIONE[s],
            }))}
          />
          <Select
            mode="multiple"
            allowClear
            placeholder="Regime"
            style={{ minWidth: 180 }}
            value={regimiFiltro}
            onChange={setRegimiFiltro}
            options={Object.values(Regime).map((r) => ({ value: r, label: ETICHETTE_REGIME[r] }))}
          />
          <DatePicker.RangePicker
            format="DD/MM/YYYY"
            placeholder={["Dal", "Al"]}
            value={periodo}
            onChange={(valori) => setPeriodo(valori as [Dayjs, Dayjs] | null)}
          />
          <Button icon={<ReloadOutlined />} onClick={resetFiltri} disabled={!filtriAttivi}>
            Reimposta filtri
          </Button>
        </Space>
        <Segmented options={OPZIONI_VISTA} value={vista} onChange={(v) => setVista(v as "tabella" | "calendario")} />
      </Space>

      <Typography.Paragraph type="secondary">
        {prenotazioniFiltrate.length} prenotazioni{filtriAttivi ? ` (su ${prenotazioni.length})` : ""}
      </Typography.Paragraph>

      {vista === "tabella" ? (
        <Table
          dataSource={prenotazioniFiltrate}
          rowKey="id"
          pagination={{ pageSize: 10, showSizeChanger: false }}
          locale={{ emptyText }}
          columns={[
            { title: "Paziente", dataIndex: "pazienteNomeCompleto" },
            ...(mostraMedico ? [{ title: "Medico", dataIndex: "medicoNomeCompleto" }] : []),
            { title: "Prestazione", dataIndex: "prestazioneNome" },
            {
              title: "Data e ora",
              key: "dataora",
              sorter: (a: Prenotazione, b: Prenotazione) =>
                dayjs(a.dataOraInizio).valueOf() - dayjs(b.dataOraInizio).valueOf(),
              render: (_: unknown, p: Prenotazione) =>
                `${dayjs(p.dataOraInizio).format("DD/MM/YYYY HH:mm")} – ${dayjs(p.dataOraFine).format("HH:mm")}`,
            },
            {
              title: "Regime",
              dataIndex: "regime",
              render: (regime: Prenotazione["regime"]) => ETICHETTE_REGIME[regime],
            },
            {
              title: "Stato",
              dataIndex: "stato",
              render: (stato: StatoPrenotazione) => (
                <Tag color={COLORE_STATO[stato]}>{ETICHETTE_STATO_PRENOTAZIONE[stato]}</Tag>
              ),
            },
            {
              title: "Azioni",
              key: "azioni",
              render: (_: unknown, p: Prenotazione) =>
                p.stato === StatoPrenotazione.Confermata ? (
                  <Popconfirm
                    title="Annullare la prenotazione?"
                    okText="Annulla prenotazione"
                    cancelText="No"
                    onConfirm={() => onAnnulla(p)}
                  >
                    <Button size="small" danger>
                      Annulla
                    </Button>
                  </Popconfirm>
                ) : null,
            },
          ]}
        />
      ) : (
        <CalendarioPrenotazioni prenotazioni={prenotazioniFiltrate} mostraMedico={mostraMedico} />
      )}
    </div>
  );
}
