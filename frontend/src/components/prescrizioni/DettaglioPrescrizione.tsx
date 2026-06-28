import { Descriptions, Modal, Table, Tag } from "antd";
import dayjs from "dayjs";
import {
  ETICHETTE_TIPO_PRESCRIZIONE,
  TipoPrescrizione,
  type Prescrizione,
} from "../../types/prescrizioni";

interface Props {
  prescrizione: Prescrizione | null;
  // Mostra anche il medico autore (utile al Paziente); per il Medico l'autore è implicito.
  mostraMedico?: boolean;
  onChiudi: () => void;
}

export function DettaglioPrescrizione({ prescrizione, mostraMedico = false, onChiudi }: Props) {
  const isPianoTerapeutico = prescrizione?.tipo === TipoPrescrizione.PianoTerapeutico;

  return (
    <Modal
      open={prescrizione !== null}
      title="Dettaglio prescrizione"
      onCancel={onChiudi}
      footer={null}
      width={680}
    >
      {prescrizione && (
        <>
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Tipo">
              <Tag color={isPianoTerapeutico ? "purple" : "blue"}>
                {ETICHETTE_TIPO_PRESCRIZIONE[prescrizione.tipo]}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Paziente">{prescrizione.pazienteNomeCompleto}</Descriptions.Item>
            {mostraMedico && (
              <Descriptions.Item label="Medico">{prescrizione.medicoNomeCompleto}</Descriptions.Item>
            )}
            {prescrizione.diagnosi && (
              <Descriptions.Item label="Diagnosi">
                <span style={{ whiteSpace: "pre-wrap" }}>{prescrizione.diagnosi}</span>
              </Descriptions.Item>
            )}
            <Descriptions.Item label="Data emissione">
              {dayjs(prescrizione.dataEmissione).format("DD/MM/YYYY")}
            </Descriptions.Item>
            <Descriptions.Item label="Data scadenza">
              {dayjs(prescrizione.dataScadenza).format("DD/MM/YYYY")}
            </Descriptions.Item>
            {isPianoTerapeutico && prescrizione.durataGiorni != null && (
              <Descriptions.Item label="Durata prevista">{prescrizione.durataGiorni} giorni</Descriptions.Item>
            )}
            {isPianoTerapeutico && prescrizione.monitoraggio && (
              <Descriptions.Item label="Monitoraggio">
                <span style={{ whiteSpace: "pre-wrap" }}>{prescrizione.monitoraggio}</span>
              </Descriptions.Item>
            )}
            {prescrizione.note && (
              <Descriptions.Item label="Note">
                <span style={{ whiteSpace: "pre-wrap" }}>{prescrizione.note}</span>
              </Descriptions.Item>
            )}
            <Descriptions.Item label="Notifica al paziente">
              <Tag color={prescrizione.notificaInviata ? "green" : "default"}>
                {prescrizione.notificaInviata ? "Inviata" : "Non inviata"}
              </Tag>
            </Descriptions.Item>
          </Descriptions>

          <Table
            style={{ marginTop: 16 }}
            dataSource={prescrizione.righe}
            rowKey={(_, index) => String(index)}
            pagination={false}
            size="small"
            locale={{ emptyText: "Nessun farmaco." }}
            columns={[
              { title: "Farmaco", dataIndex: "farmaco" },
              { title: "Posologia", dataIndex: "posologia" },
              { title: "Q.tà", dataIndex: "quantita", width: 80 },
            ]}
          />
        </>
      )}
    </Modal>
  );
}
