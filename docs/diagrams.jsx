import { useState } from "react";

const AUDIT_FIELDS = [
  { name: "CreatedAt", type: "datetime", audit: true },
  { name: "CreatedBy", type: "string", audit: true },
  { name: "ModifiedAt", type: "datetime?", audit: true },
  { name: "ModifiedBy", type: "string?", audit: true },
];

const ER_ENTITIES = [
  {
    name: "AppUser",
    color: "#475569",
    identity: true,
    note: "extends IdentityUser",
    attrs: [
      { name: "Id", type: "PK", pk: true },
      { name: "Nome", type: "string" },
      { name: "Cognome", type: "string" },
      { name: "Email", type: "string" },
      { name: "UserName", type: "string" },
      { name: "PasswordHash", type: "string" },
    ],
  },
  {
    name: "Paziente",
    color: "#2563eb",
    attrs: [
      { name: "PazienteId", type: "PK", pk: true },
      { name: "UserId", type: "FK", fk: true },
      { name: "CodiceFiscale", type: "string" },
      { name: "DataNascita", type: "date" },
      { name: "Telefono", type: "string" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Medico",
    color: "#0891b2",
    attrs: [
      { name: "MedicoId", type: "PK", pk: true },
      { name: "UserId", type: "FK", fk: true },
      { name: "Specializzazione", type: "string" },
      { name: "ServizioId", type: "FK", fk: true },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Servizio",
    color: "#7c3aed",
    attrs: [
      { name: "ServizioId", type: "PK", pk: true },
      { name: "Nome", type: "string" },
      { name: "Descrizione", type: "string" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Prestazione",
    color: "#0d9488",
    attrs: [
      { name: "PrestazioneId", type: "PK", pk: true },
      { name: "ServizioId", type: "FK", fk: true },
      { name: "Nome", type: "string" },
      { name: "Descrizione", type: "string" },
      { name: "DurataMinuti", type: "int" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Tariffa",
    color: "#854d0e",
    attrs: [
      { name: "TariffaId", type: "PK", pk: true },
      { name: "PrestazioneId", type: "FK", fk: true },
      { name: "Regime", type: "enum" },
      { name: "Prezzo", type: "decimal" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Turno",
    color: "#065f46",
    attrs: [
      { name: "TurnoId", type: "PK", pk: true },
      { name: "MedicoId", type: "FK", fk: true },
      { name: "PrestazioneId", type: "FK", fk: true },
      { name: "GiornoSettimana", type: "enum" },
      { name: "OraInizio", type: "time" },
      { name: "OraFine", type: "time" },
      { name: "DurataSlotMin", type: "int" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Slot",
    color: "#047857",
    note: "materializzato dal Turno",
    attrs: [
      { name: "SlotId", type: "PK", pk: true },
      { name: "TurnoId", type: "FK", fk: true },
      { name: "DataOraInizio", type: "datetime" },
      { name: "DataOraFine", type: "datetime" },
      { name: "Stato", type: "enum" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Prenotazione",
    color: "#059669",
    attrs: [
      { name: "PrenotazioneId", type: "PK", pk: true },
      { name: "PazienteId", type: "FK", fk: true },
      { name: "SlotId", type: "FK", fk: true },
      { name: "Regime", type: "enum" },
      { name: "Stato", type: "enum" },
      { name: "ConfermataDalPaziente", type: "bool" },
      { name: "Note", type: "string" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Referto",
    color: "#d97706",
    attrs: [
      { name: "RefertoId", type: "PK", pk: true },
      { name: "PrenotazioneId", type: "FK", fk: true },
      { name: "DataEmissione", type: "datetime" },
      { name: "Contenuto", type: "text" },
      { name: "FilePath", type: "string" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Prescrizione",
    color: "#dc2626",
    attrs: [
      { name: "PrescrizioneId", type: "PK", pk: true },
      { name: "PazienteId", type: "FK", fk: true },
      { name: "MedicoId", type: "FK", fk: true },
      { name: "Tipo", type: "enum" },
      { name: "Diagnosi", type: "string?" },
      { name: "DurataGiorni", type: "int?" },
      { name: "Monitoraggio", type: "string?" },
      { name: "DataEmissione", type: "date" },
      { name: "DataScadenza", type: "date" },
      { name: "Note", type: "string" },
      { name: "NotificaInviata", type: "bool" },
      { name: "OriginAssistita", type: "bool" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "RigaPrescrizione",
    color: "#b91c1c",
    attrs: [
      { name: "RigaPrescrizioneId", type: "PK", pk: true },
      { name: "PrescrizioneId", type: "FK", fk: true },
      { name: "Farmaco", type: "string" },
      { name: "Posologia", type: "string" },
      { name: "Quantita", type: "int" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Fattura",
    color: "#be185d",
    attrs: [
      { name: "FatturaId", type: "PK", pk: true },
      { name: "PrenotazioneId", type: "FK", fk: true },
      { name: "PazienteId", type: "FK", fk: true },
      { name: "Importo", type: "decimal" },
      { name: "Regime", type: "enum" },
      { name: "DataEmissione", type: "date" },
      { name: "Stato", type: "enum" },
      ...AUDIT_FIELDS,
    ],
  },
  {
    name: "Notifica",
    color: "#9333ea",
    note: "promemoria / avvisi al paziente",
    attrs: [
      { name: "NotificaId", type: "PK", pk: true },
      { name: "DestinatarioUserId", type: "FK", fk: true },
      { name: "Tipo", type: "enum" },
      { name: "Titolo", type: "string" },
      { name: "Messaggio", type: "string" },
      { name: "RiferimentoId", type: "guid?" },
      { name: "Letta", type: "bool" },
      { name: "StatoInvio", type: "enum" },
      { name: "DataInvio", type: "datetime?" },
      { name: "Canale", type: "enum" },
      ...AUDIT_FIELDS,
    ],
  },
];

const ER_RELATIONS = [
  { from: "AppUser",      to: "Paziente",     label: "1..1", fromSide: "bottom", toSide: "top"   },
  { from: "AppUser",      to: "Medico",        label: "1..1", fromSide: "bottom", toSide: "top"   },
  { from: "Medico",       to: "Servizio",      label: "N..1", fromSide: "right",  toSide: "left"  },
  { from: "Servizio",     to: "Prestazione",   label: "1..N", fromSide: "bottom", toSide: "top"   },
  { from: "Prestazione",  to: "Tariffa",       label: "1..N", fromSide: "right",  toSide: "left"  },
  { from: "Prestazione",  to: "Turno",         label: "1..N", fromSide: "left",   toSide: "right" },
  { from: "Medico",       to: "Turno",         label: "1..N", fromSide: "bottom", toSide: "top"   },
  { from: "Turno",        to: "Slot",          label: "1..N", fromSide: "bottom", toSide: "top"   },
  { from: "Slot",         to: "Prenotazione",  label: "1..1", fromSide: "left",   toSide: "right" },
  { from: "Paziente",     to: "Prenotazione",  label: "1..N", fromSide: "bottom", toSide: "left"  },
  { from: "Prenotazione", to: "Referto",       label: "1..1", fromSide: "bottom", toSide: "top"   },
  { from: "Prenotazione", to: "Fattura",       label: "1..1", fromSide: "bottom", toSide: "top"   },
  { from: "Paziente",     to: "Prescrizione",  label: "1..N", fromSide: "bottom", toSide: "top"   },
  { from: "Medico",       to: "Prescrizione",  label: "1..N", fromSide: "bottom", toSide: "top"   },
  { from: "Prescrizione", to: "RigaPrescrizione", label: "1..N", fromSide: "right", toSide: "left" },
  { from: "AppUser",      to: "Notifica",      label: "1..N", fromSide: "bottom", toSide: "top"   },
];

const USE_CASES = {
  actors: ["Paziente", "Medico", "Amministratore", "Sistema AI (Mistral)"],
  actorColors: { "Paziente": "#2563eb", "Medico": "#0891b2", "Amministratore": "#7c3aed", "Sistema AI (Mistral)": "#4f46e5" },
  cases: [
    { id: "UC01", name: "Registrazione / Login",                       actors: ["Paziente", "Medico", "Amministratore"] },
    { id: "UC02", name: "Cerca prestazione e slot disponibili",        actors: ["Paziente"] },
    { id: "UC03", name: "Prenota visita (sceglie turno)",              actors: ["Paziente"] },
    { id: "UC04", name: "Prenota per un paziente",                     actors: ["Medico", "Amministratore"] },
    { id: "UC05", name: "Visualizza / annulla prenotazione",           actors: ["Paziente", "Medico", "Amministratore"] },
    { id: "UC06", name: "Conferma presenza alla visita",               actors: ["Paziente"] },
    { id: "UC07", name: "Visualizza agenda e calendario turni",        actors: ["Medico", "Amministratore"] },
    { id: "UC08", name: "Gestisci turni ambulatorio",                  actors: ["Amministratore"] },
    { id: "UC09", name: "Carica referto",                              actors: ["Medico"] },
    { id: "UC10", name: "Visualizza / scarica referti",                actors: ["Paziente", "Medico"] },
    { id: "UC11", name: "Emetti prescrizione",                         actors: ["Medico"] },
    { id: "UC12", name: "Assistenza AI redazione prescrizione (HITL)", actors: ["Medico", "Sistema AI (Mistral)"] },
    { id: "UC13", name: "Visualizza storico prescrizioni",             actors: ["Paziente", "Medico"] },
    { id: "UC14", name: "Ricevi notifiche (promemoria e prescrizioni)", actors: ["Paziente"] },
    { id: "UC15", name: "Eroga visita",                                actors: ["Medico", "Amministratore"] },
    { id: "UC16", name: "Genera fattura (completa prenotazione)",      actors: ["Amministratore"] },
    { id: "UC17", name: "Visualizza proprie fatture",                  actors: ["Paziente"] },
    { id: "UC18", name: "Gestione fatture (elenco completo)",          actors: ["Amministratore"] },
    { id: "UC19", name: "Gestione servizi / prestazioni / tariffe",    actors: ["Amministratore"] },
    { id: "UC20", name: "Gestione medici",                             actors: ["Amministratore"] },
  ],
};

const CLASS_DIAGRAM = [
  { name: "AuthController",          type: "Controller", color: "#475569", methods: ["POST /auth/register", "POST /auth/login", "GET /auth/me"] },
  { name: "PazienteController",      type: "Controller", color: "#2563eb", methods: ["GET /pazienti"] },
  { name: "MedicoController",        type: "Controller", color: "#0891b2", methods: ["GET /medici", "GET /medici/{id}", "POST /medici", "PUT /medici/{id}", "POST /medici/{id}/reset-password"] },
  { name: "ServizioController",      type: "Controller", color: "#7c3aed", methods: ["GET /servizi", "GET /servizi/{id}", "GET /servizi/{id}/prestazioni", "POST /servizi", "PUT /servizi/{id}"] },
  { name: "PrestazioneController",   type: "Controller", color: "#0d9488", methods: ["GET /prestazioni", "GET /prestazioni/{id}", "POST /prestazioni", "PUT /prestazioni/{id}"] },
  { name: "TariffaController",       type: "Controller", color: "#854d0e", methods: ["GET /tariffe/{id}", "GET /tariffe/prestazione/{id}", "POST /tariffe", "PUT /tariffe/{id}", "DELETE /tariffe/{id}"] },
  { name: "TurnoController",         type: "Controller", color: "#065f46", methods: ["GET /turni", "GET /turni/{id}", "GET /turni/medico/{medicoId}", "GET /turni/miei", "POST /turni", "PUT /turni/{id}", "DELETE /turni/{id}"] },
  { name: "SlotController",          type: "Controller", color: "#047857", methods: ["GET /slot/prestazione/{prestazioneId}"] },
  { name: "PrenotazioneController",  type: "Controller", color: "#059669", methods: ["POST /prenotazioni", "GET /prenotazioni", "GET /prenotazioni/{id}", "GET /prenotazioni/mie", "GET /prenotazioni/agenda", "PUT /prenotazioni/{id}/annulla", "PUT /prenotazioni/{id}/conferma-presenza", "PUT /prenotazioni/{id}/eroga", "PUT /prenotazioni/{id}/completa"] },
  { name: "RefertoController",       type: "Controller", color: "#d97706", methods: ["POST /referti", "GET /referti/{id}", "GET /referti/prenotazione/{prenotazioneId}", "GET /referti/{id}/file"] },
  { name: "PrescrizioneController",  type: "Controller", color: "#dc2626", methods: ["POST /prescrizioni", "GET /prescrizioni/{id}", "GET /prescrizioni/mie", "GET /prescrizioni/emesse"] },
  { name: "FatturaController",       type: "Controller", color: "#be185d", methods: ["GET /fatture", "GET /fatture/{id}", "GET /fatture/mie"] },
  { name: "NotificaController",      type: "Controller", color: "#9333ea", methods: ["GET /notifiche/mie", "GET /notifiche/non-lette/count", "PUT /notifiche/{id}/letta", "POST /notifiche/genera-promemoria"] },
  { name: "AiController",            type: "Controller", color: "#4f46e5", methods: ["POST /ai/suggerisci"] },
  { name: "MistralService",          type: "Service",    color: "#4f46e5", methods: ["SuggerisciAsync(DatiClinici)", "ModalitaDemo (stub se manca ApiKey)"] },
  { name: "FileStorageService",      type: "Service",    color: "#d97706", methods: ["SaveAsync()", "OpenReadAsync()", "DeleteAsync()"] },
  { name: "NotificationSender",      type: "Service",    color: "#9333ea", methods: ["SendAsync()  (LoggingNotificationSender: in-app + log)", "Email/SMS: evoluzione futura"] },
  { name: "PromemoriaBackgroundService", type: "Worker", color: "#9333ea", methods: ["ExecuteAsync()  (PeriodicTimer)", "GeneraPromemoriaDovutiAsync()"] },
  { name: "AppDbContext",            type: "DbContext",  color: "#374151", methods: ["DbSet<Paziente>", "DbSet<Medico>", "DbSet<Servizio>", "DbSet<Prestazione>", "DbSet<Tariffa>", "DbSet<Turno>", "DbSet<Slot>", "DbSet<Prenotazione>", "DbSet<Referto>", "DbSet<Prescrizione>", "DbSet<RigaPrescrizione>", "DbSet<Fattura>", "DbSet<Notifica>", "SaveChangesAsync() [audit]"] },
];

// ---- Layout positions ----
const ER_POS = {
  AppUser:      { x: 500, y: 20   },
  Paziente:     { x: 20,  y: 248  },
  Medico:       { x: 500, y: 248  },
  Servizio:     { x: 740, y: 248  },
  Prescrizione: { x: 20,  y: 542  },
  RigaPrescrizione: { x: 260, y: 542 },
  Turno:        { x: 500, y: 542  },
  Prestazione:  { x: 740, y: 542  },
  Tariffa:      { x: 980, y: 542  },
  Slot:         { x: 500, y: 900  },
  Prenotazione: { x: 220, y: 900  },
  Referto:      { x: 20,  y: 1240 },
  Fattura:      { x: 260, y: 1240 },
  Notifica:     { x: 960, y: 1240 },
};

const CARD_W = 200;
const ATTR_H = 22;
const HEADER_H = 38;

function erCardHeight(entity) {
  return HEADER_H + entity.attrs.length * ATTR_H + 8;
}

function erEdgePoint(name, side) {
  const pos = ER_POS[name];
  const entity = ER_ENTITIES.find(e => e.name === name);
  const h = erCardHeight(entity);
  const cx = pos.x + CARD_W / 2;
  const cy = pos.y + h / 2;
  if (side === "right")  return { x: pos.x + CARD_W, y: cy };
  if (side === "left")   return { x: pos.x, y: cy };
  if (side === "bottom") return { x: cx, y: pos.y + h };
  if (side === "top")    return { x: cx, y: pos.y };
  return { x: cx, y: cy };
}

function ERDiagram() {
  const [hovered, setHovered] = useState(null);
  const SVG_W = 1220;
  const SVG_H = 1700;

  return (
    <div style={{ overflowX: "auto" }}>
      <svg viewBox={`0 0 ${SVG_W} ${SVG_H}`} style={{ minWidth: 900, width: "100%", background: "#f8fafc", borderRadius: 12, border: "1px solid #e2e8f0" }}>
        <defs>
          <marker id="arrow" markerWidth="8" markerHeight="8" refX="6" refY="3" orient="auto">
            <path d="M0,0 L0,6 L8,3 z" fill="#94a3b8" />
          </marker>
          <marker id="arrow-id" markerWidth="8" markerHeight="8" refX="6" refY="3" orient="auto">
            <path d="M0,0 L0,6 L8,3 z" fill="#475569" />
          </marker>
        </defs>

        {/* Identity background */}
        <rect x={470} y={8} width={250} height={erCardHeight(ER_ENTITIES.find(e => e.name === "AppUser")) + 20}
          rx={12} fill="#f1f5f9" stroke="#cbd5e1" strokeWidth="1.5" strokeDasharray="6,3" />
        <text x={478} y={22} fontSize="9" fill="#94a3b8" fontFamily="monospace">ASP.NET Core Identity</text>

        {/* Relations */}
        {ER_RELATIONS.map((rel, i) => {
          const a = erEdgePoint(rel.from, rel.fromSide);
          const b = erEdgePoint(rel.to, rel.toSide);
          const mx = (a.x + b.x) / 2;
          const my = (a.y + b.y) / 2;
          const isId = rel.from === "AppUser" || rel.to === "AppUser";
          return (
            <g key={i}>
              <line x1={a.x} y1={a.y} x2={b.x} y2={b.y}
                stroke={isId ? "#475569" : "#94a3b8"} strokeWidth="1.5"
                strokeDasharray={isId ? "4,2" : "5,3"}
                markerEnd={isId ? "url(#arrow-id)" : "url(#arrow)"} />
              <rect x={mx - 20} y={my - 10} width={40} height={18} rx={4} fill="white" stroke="#e2e8f0" />
              <text x={mx} y={my + 4} textAnchor="middle" fontSize="10" fill="#64748b" fontFamily="monospace">{rel.label}</text>
            </g>
          );
        })}

        {/* Entity cards */}
        {ER_ENTITIES.map(entity => {
          const pos = ER_POS[entity.name];
          const h = erCardHeight(entity);
          const isHov = hovered === entity.name;
          return (
            <g key={entity.name}
              onMouseEnter={() => setHovered(entity.name)}
              onMouseLeave={() => setHovered(null)}
              style={{ cursor: "default", filter: isHov ? "drop-shadow(0 4px 12px rgba(0,0,0,0.18))" : "none" }}>
              <rect x={pos.x} y={pos.y} width={CARD_W} height={h} rx={8}
                fill="white" stroke={entity.color} strokeWidth={isHov ? 2.5 : 1.5} />
              <rect x={pos.x} y={pos.y} width={CARD_W} height={HEADER_H} rx={8} fill={entity.color} />
              <rect x={pos.x} y={pos.y + HEADER_H - 8} width={CARD_W} height={8} fill={entity.color} />
              <text x={pos.x + CARD_W / 2} y={pos.y + (entity.note ? HEADER_H / 2 : HEADER_H / 2 + 6)}
                textAnchor="middle" fontSize="13" fontWeight="700" fill="white" fontFamily="system-ui">{entity.name}</text>
              {entity.note && (
                <text x={pos.x + CARD_W / 2} y={pos.y + HEADER_H / 2 + 13}
                  textAnchor="middle" fontSize="8" fill="rgba(255,255,255,0.75)" fontFamily="monospace">{entity.note}</text>
              )}
              {entity.attrs.map((attr, ai) => (
                <g key={attr.name}>
                  <line x1={pos.x + 8} y1={pos.y + HEADER_H + ai * ATTR_H + 1}
                    x2={pos.x + CARD_W - 8} y2={pos.y + HEADER_H + ai * ATTR_H + 1}
                    stroke="#f1f5f9" strokeWidth="1" />
                  <text x={pos.x + 10} y={pos.y + HEADER_H + ai * ATTR_H + 15}
                    fontSize="10"
                    fill={attr.pk ? entity.color : attr.fk ? "#64748b" : attr.audit ? "#f59e0b" : "#374151"}
                    fontFamily="monospace" fontWeight={attr.pk ? "700" : "400"}>
                    {attr.pk ? "🔑 " : attr.fk ? "🔗 " : attr.audit ? "📋 " : "   "}{attr.name}
                  </text>
                  <text x={pos.x + CARD_W - 8} y={pos.y + HEADER_H + ai * ATTR_H + 15}
                    fontSize="9" fill="#94a3b8" textAnchor="end" fontFamily="monospace">{attr.type}</text>
                </g>
              ))}
            </g>
          );
        })}

        {/* Amministratore note */}
        <rect x={560} y={1240} width={300} height={28} rx={8}
          fill="#f8fafc" stroke="#cbd5e1" strokeWidth="1.5" strokeDasharray="4,2" />
        <text x={710} y={1252} textAnchor="middle" fontSize="11" fontWeight="700" fill="#475569" fontFamily="system-ui">Amministratore</text>
        <text x={710} y={1264} textAnchor="middle" fontSize="9" fill="#94a3b8" fontFamily="monospace">solo ruolo Identity — nessuna entità dominio</text>
      </svg>
    </div>
  );
}

function UseCaseDiagram() {
  const [sel, setSel] = useState(null);
  const { actors, actorColors, cases } = USE_CASES;
  const filtered = sel ? cases.filter(c => c.actors.includes(sel)) : cases;
  return (
    <div style={{ fontFamily: "system-ui" }}>
      <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 16 }}>
        <span style={{ fontSize: 12, color: "#64748b", alignSelf: "center" }}>Filtra per attore:</span>
        {actors.map(a => (
          <button key={a} onClick={() => setSel(sel === a ? null : a)}
            style={{ padding: "4px 12px", borderRadius: 20, border: "none", cursor: "pointer", fontSize: 12,
              background: sel === a ? actorColors[a] : "#f1f5f9",
              color: sel === a ? "white" : "#374151",
              fontWeight: sel === a ? "700" : "400", transition: "all 0.2s" }}>{a}</button>
        ))}
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 10 }}>
        {filtered.map(uc => (
          <div key={uc.id} style={{ background: "white", border: "1px solid #e2e8f0", borderRadius: 10,
            padding: "12px 14px", borderLeft: `4px solid ${actorColors[uc.actors[0]]}` }}>
            <div style={{ fontSize: 11, color: "#94a3b8", fontFamily: "monospace", marginBottom: 4 }}>{uc.id}</div>
            <div style={{ fontSize: 13, fontWeight: 600, color: "#1e293b", marginBottom: 8 }}>{uc.name}</div>
            <div style={{ display: "flex", flexWrap: "wrap", gap: 4 }}>
              {uc.actors.map(a => (
                <span key={a} style={{ fontSize: 10, padding: "2px 8px", borderRadius: 10,
                  background: actorColors[a] + "20", color: actorColors[a], fontWeight: 600 }}>{a}</span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function ClassDiagram() {
  const typeIcons = { Controller: "⚙️", Service: "🤖", DbContext: "🗄️", Worker: "⏱️" };
  return (
    <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 12 }}>
      {CLASS_DIAGRAM.map(cls => (
        <div key={cls.name} style={{ background: "white", border: `1.5px solid ${cls.color}`,
          borderRadius: 10, overflow: "hidden", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
          <div style={{ background: cls.color, padding: "10px 14px" }}>
            <div style={{ fontSize: 10, color: "rgba(255,255,255,0.7)", marginBottom: 2 }}>{typeIcons[cls.type]} {cls.type}</div>
            <div style={{ fontSize: 13, fontWeight: 700, color: "white", fontFamily: "monospace" }}>{cls.name}</div>
          </div>
          <div style={{ padding: "8px 0" }}>
            {cls.methods.map((m, i) => (
              <div key={i} style={{ padding: "4px 14px", fontSize: 11, fontFamily: "monospace", color: "#374151",
                borderBottom: i < cls.methods.length - 1 ? "1px solid #f1f5f9" : "none" }}>{m}</div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}

export default function App() {
  const [tab, setTab] = useState("er");
  const tabs = [
    { id: "er",      label: "📊 Diagramma ER" },
    { id: "usecase", label: "👤 Use Case (UML)" },
    { id: "class",   label: "🏗️ Classi / API (UML)" },
  ];
  return (
    <div style={{ minHeight: "100vh", background: "#f1f5f9", padding: 24, fontFamily: "system-ui" }}>
      <div style={{ maxWidth: 1100, margin: "0 auto" }}>
        <div style={{ marginBottom: 24 }}>
          <div style={{ fontSize: 11, color: "#64748b", fontFamily: "monospace", marginBottom: 4 }}>
            Project Work — L-31 Informatica per le Aziende Digitali
          </div>
          <h1 style={{ fontSize: 22, fontWeight: 800, color: "#1e293b", margin: 0 }}>MediCore — Diagrammi di Progetto</h1>
          <p style={{ fontSize: 13, color: "#64748b", margin: "6px 0 0" }}>Applicazione full-stack API-based per la gestione sanitaria ambulatoriale</p>
        </div>
        <div style={{ display: "flex", gap: 4, marginBottom: 20, background: "white", padding: 4, borderRadius: 10,
          width: "fit-content", boxShadow: "0 1px 3px rgba(0,0,0,0.08)" }}>
          {tabs.map(t => (
            <button key={t.id} onClick={() => setTab(t.id)}
              style={{ padding: "8px 18px", borderRadius: 8, border: "none", cursor: "pointer", fontSize: 13,
                background: tab === t.id ? "#1e293b" : "transparent",
                color: tab === t.id ? "white" : "#64748b",
                fontWeight: tab === t.id ? 700 : 400, transition: "all 0.2s" }}>{t.label}</button>
          ))}
        </div>
        <div style={{ background: "white", borderRadius: 12, padding: 20, boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
          {tab === "er" && (
            <>
              <h2 style={{ fontSize: 15, fontWeight: 700, color: "#1e293b", marginBottom: 4, marginTop: 0 }}>Entity Relationship Diagram</h2>
              <p style={{ fontSize: 12, color: "#64748b", marginBottom: 16 }}>
                Hover su un'entità per evidenziarla. Il paziente prenota una <strong>Prestazione</strong> tramite un <strong>Turno</strong> — non sceglie il medico direttamente.
                Il <strong>Regime</strong> (SSN/Privato/Assicurativo) è sulla Prenotazione e sulla Fattura.
              </p>
              <ERDiagram />
              <div style={{ marginTop: 16, display: "flex", flexWrap: "wrap", gap: 8 }}>
                {ER_ENTITIES.map(e => (
                  <span key={e.name} style={{ fontSize: 11, padding: "3px 10px", borderRadius: 20,
                    background: e.color + "15", color: e.color, fontWeight: 600 }}>{e.name}</span>
                ))}
              </div>
            </>
          )}
          {tab === "usecase" && (
            <>
              <h2 style={{ fontSize: 15, fontWeight: 700, color: "#1e293b", marginBottom: 4, marginTop: 0 }}>Diagramma dei Casi d'Uso</h2>
              <p style={{ fontSize: 12, color: "#64748b", marginBottom: 16 }}>Clicca su un attore per filtrare.</p>
              <UseCaseDiagram />
            </>
          )}
          {tab === "class" && (
            <>
              <h2 style={{ fontSize: 15, fontWeight: 700, color: "#1e293b", marginBottom: 4, marginTop: 0 }}>Diagramma delle Classi — Controller & Services</h2>
              <p style={{ fontSize: 12, color: "#64748b", marginBottom: 16 }}>Controller REST, servizi e DbContext del backend .NET.</p>
              <ClassDiagram />
            </>
          )}
        </div>
        <div style={{ marginTop: 16, display: "flex", gap: 16, fontSize: 11, color: "#94a3b8", flexWrap: "wrap" }}>
          <span>🔑 Primary Key</span>
          <span>🔗 Foreign Key</span>
          <span style={{ color: "#f59e0b" }}>📋 Audit Field</span>
          <span>⚙️ Controller REST</span>
          <span>🤖 Service</span>
          <span>⏱️ Worker in background</span>
          <span>🗄️ DbContext EF Core</span>
        </div>
      </div>
    </div>
  );
}
