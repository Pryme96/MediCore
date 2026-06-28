import { useMemo, useState } from "react";
import { Badge, Button, Calendar, Empty, Select, Space, Typography } from "antd";
import dayjs, { type Dayjs } from "dayjs";
import type { Slot } from "../../types/prenotazioni";

interface CalendarioSlotProps {
  slots: Slot[];
  slotSelezionato: Slot | null;
  onSelect: (slot: Slot) => void;
}

const chiaveGiorno = (data: string | Dayjs) => dayjs(data).format("YYYY-MM-DD");

export function CalendarioSlot({ slots, slotSelezionato, onSelect }: CalendarioSlotProps) {
  const [medicoId, setMedicoId] = useState<string | null>(null);
  const [giornoSelezionato, setGiornoSelezionato] = useState<Dayjs | null>(null);

  // Medici presenti negli slot disponibili (per il filtro opzionale).
  const medici = useMemo(() => {
    const mappa = new Map<string, string>();
    slots.forEach((s) => mappa.set(s.medicoId, s.medicoNomeCompleto));
    return Array.from(mappa, ([id, nome]) => ({ id, nome }));
  }, [slots]);

  const slotFiltrati = useMemo(
    () => (medicoId ? slots.filter((s) => s.medicoId === medicoId) : slots),
    [slots, medicoId]
  );

  // Slot raggruppati per giorno (YYYY-MM-DD).
  const slotPerGiorno = useMemo(() => {
    const mappa = new Map<string, Slot[]>();
    slotFiltrati.forEach((s) => {
      const chiave = chiaveGiorno(s.dataOraInizio);
      const lista = mappa.get(chiave) ?? [];
      lista.push(s);
      mappa.set(chiave, lista);
    });
    return mappa;
  }, [slotFiltrati]);

  const slotDelGiorno = giornoSelezionato
    ? (slotPerGiorno.get(chiaveGiorno(giornoSelezionato)) ?? []).slice().sort(
        (a, b) => dayjs(a.dataOraInizio).valueOf() - dayjs(b.dataOraInizio).valueOf()
      )
    : [];

  if (slots.length === 0) {
    return <Empty description="Nessuno slot disponibile per questa prestazione nei prossimi giorni." />;
  }

  return (
    <div>
      <Space style={{ marginBottom: 12 }}>
        <Typography.Text>Filtra per medico:</Typography.Text>
        <Select
          allowClear
          placeholder="Tutti i medici"
          style={{ minWidth: 240 }}
          value={medicoId}
          onChange={(value) => {
            setMedicoId(value ?? null);
            setGiornoSelezionato(null);
          }}
          options={medici.map((m) => ({ value: m.id, label: m.nome }))}
        />
      </Space>

      <Calendar
        fullscreen={false}
        value={giornoSelezionato ?? undefined}
        disabledDate={(data) => !slotPerGiorno.has(chiaveGiorno(data))}
        onSelect={(data, info) => {
          if (info.source === "date" && slotPerGiorno.has(chiaveGiorno(data))) {
            setGiornoSelezionato(data);
          }
        }}
        cellRender={(data) => {
          const quanti = slotPerGiorno.get(chiaveGiorno(data))?.length ?? 0;
          return quanti > 0 ? <Badge status="success" /> : null;
        }}
      />

      <div style={{ marginTop: 16 }}>
        {!giornoSelezionato ? (
          <Typography.Text type="secondary">
            Seleziona un giorno con disponibilità per vedere gli orari.
          </Typography.Text>
        ) : slotDelGiorno.length === 0 ? (
          <Typography.Text type="secondary">Nessun orario disponibile in questo giorno.</Typography.Text>
        ) : (
          <>
            <Typography.Title level={5}>
              Orari disponibili — {giornoSelezionato.format("DD/MM/YYYY")}
            </Typography.Title>
            <Space wrap>
              {slotDelGiorno.map((slot) => (
                <Button
                  key={slot.id}
                  type={slotSelezionato?.id === slot.id ? "primary" : "default"}
                  onClick={() => onSelect(slot)}
                >
                  {dayjs(slot.dataOraInizio).format("HH:mm")} — {slot.medicoNomeCompleto}
                </Button>
              ))}
            </Space>
          </>
        )}
      </div>
    </div>
  );
}
