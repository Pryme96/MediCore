using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Ai;

namespace MediCore.Api.Infrastructure;

public class MistralService(HttpClient http, IConfiguration config, ILogger<MistralService> logger) : IMistralService
{
    private readonly string? _apiKey = config["Mistral:ApiKey"];
    private readonly string _model = config["Mistral:Model"] ?? "mistral-large-latest";

    public bool ModalitaDemo { get; } =
        config.GetValue<bool>("Mistral:DemoMode") || string.IsNullOrWhiteSpace(config["Mistral:ApiKey"]);

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<SuggerimentoOpzione>> SuggerisciAsync(DatiClinici dati, CancellationToken ct = default)
    {
        if (ModalitaDemo)
            return StubOpzioni(dati);

        try
        {
            JsonElement schema;
            using (var doc = JsonDocument.Parse(SchemaJson))
                schema = doc.RootElement.Clone();

            var body = new
            {
                model = _model,
                temperature = 0.3,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = BuildUserPrompt(dati) }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new { name = "suggerimenti", strict = true, schema }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var chat = await response.Content.ReadFromJsonAsync<MistralChatResponse>(cancellationToken: ct);
            var contenuto = chat?.Choices?.FirstOrDefault()?.Message?.Content;
            logger.LogInformation("Suggerimenti generati con modello {Modello}", _model);

            if (string.IsNullOrWhiteSpace(contenuto))
                return [];

            var output = JsonSerializer.Deserialize<MistralOutput>(contenuto, JsonOpts);
            return output?.Opzioni ?? [];
        }
        catch (Exception ex)
        {
            // Fallback sicuro: l'assistenza non deve mai far fallire la richiesta del medico.
            logger.LogWarning(ex, "Generazione suggerimenti non riuscita: si restituisce un elenco vuoto.");
            return [];
        }
    }

    private const string SystemPrompt =
        "Sei un assistente alla redazione clinica a supporto di un medico in Italia. " +
        "A partire dai dati clinici de-identificati forniti, proponi da 2 a 3 opzioni coerenti con il tipo richiesto " +
        "(prescrizione farmacologica oppure piano terapeutico). " +
        "Rispondi esclusivamente con l'oggetto JSON conforme allo schema, in lingua italiana. " +
        "Per la prescrizione farmacologica lascia null i campi diagnosiSuggerita, durataGiorni e monitoraggio. " +
        "Per il piano terapeutico valorizza diagnosiSuggerita e, quando opportuno, durataGiorni e monitoraggio. " +
        "Per ogni opzione indica una motivazione clinica sintetica e, quando rilevante, le avvertenze di sicurezza. " +
        "Se i dati sono insufficienti o non è possibile proporre in sicurezza, restituisci un elenco di opzioni vuoto. " +
        "Le proposte sono un supporto non vincolante: la responsabilità della prescrizione resta del medico.";

    private static string BuildUserPrompt(DatiClinici dati)
    {
        var tipo = dati.Tipo == TipoPrescrizione.PianoTerapeutico ? "piano terapeutico" : "prescrizione farmacologica";
        var sesso = dati.Sesso switch
        {
            Sesso.Maschile => "M",
            Sesso.Femminile => "F",
            _ => "non specificato"
        };

        var righe = new List<string>
        {
            $"Tipo richiesto: {tipo}",
            $"Età: {dati.Eta} anni",
            $"Sesso: {sesso}",
            $"Contesto clinico: {dati.ContestoClinico}"
        };
        if (!string.IsNullOrWhiteSpace(dati.Allergie))
            righe.Add($"Allergie note: {dati.Allergie}");
        if (!string.IsNullOrWhiteSpace(dati.TerapieInCorso))
            righe.Add($"Terapie in corso: {dati.TerapieInCorso}");

        return string.Join("\n", righe);
    }

    // Schema dell'output strutturato (strict): un oggetto { opzioni: [...] }.
    private const string SchemaJson = """
    {
      "type": "object",
      "additionalProperties": false,
      "required": ["opzioni"],
      "properties": {
        "opzioni": {
          "type": "array",
          "items": {
            "type": "object",
            "additionalProperties": false,
            "required": ["righe", "diagnosiSuggerita", "durataGiorni", "monitoraggio", "motivazione", "avvertenze"],
            "properties": {
              "righe": {
                "type": "array",
                "items": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["farmaco", "posologia", "quantita"],
                  "properties": {
                    "farmaco": { "type": "string" },
                    "posologia": { "type": "string" },
                    "quantita": { "type": "integer" }
                  }
                }
              },
              "diagnosiSuggerita": { "type": ["string", "null"] },
              "durataGiorni": { "type": ["integer", "null"] },
              "monitoraggio": { "type": ["string", "null"] },
              "motivazione": { "type": "string" },
              "avvertenze": { "type": ["string", "null"] }
            }
          }
        }
      }
    }
    """;

    // Suggerimenti dimostrativi deterministici usati quando non è configurata una chiave API:
    // permettono di mostrare l'intero flusso senza rete né costi. Contenuti volutamente generici.
    private static IReadOnlyList<SuggerimentoOpzione> StubOpzioni(DatiClinici dati)
    {
        if (dati.Tipo == TipoPrescrizione.PianoTerapeutico)
        {
            return
            [
                new SuggerimentoOpzione
                {
                    Righe = [new RigaSuggerita { Farmaco = "Ramipril 5 mg", Posologia = "1 compressa al giorno, al mattino", Quantita = 2 }],
                    DiagnosiSuggerita = "Ipertensione arteriosa essenziale",
                    DurataGiorni = 180,
                    Monitoraggio = "Controllo pressorio mensile, funzione renale ed elettroliti periodici",
                    Motivazione = "ACE-inibitore di prima linea, buon profilo di tollerabilità.",
                    Avvertenze = "Monitorare potassio e creatinina; controindicato in gravidanza."
                },
                new SuggerimentoOpzione
                {
                    Righe = [new RigaSuggerita { Farmaco = "Amlodipina 5 mg", Posologia = "1 compressa al giorno", Quantita = 2 }],
                    DiagnosiSuggerita = "Ipertensione arteriosa essenziale",
                    DurataGiorni = 180,
                    Monitoraggio = "Controllo pressorio mensile",
                    Motivazione = "Calcio-antagonista, utile in alternativa o in associazione.",
                    Avvertenze = "Possibile comparsa di edemi declivi."
                }
            ];
        }

        return
        [
            new SuggerimentoOpzione
            {
                Righe = [new RigaSuggerita { Farmaco = "Paracetamolo 1000 mg", Posologia = "1 compressa ogni 8 ore al bisogno", Quantita = 1 }],
                Motivazione = "Sintomatico di prima linea con buon profilo di sicurezza.",
                Avvertenze = "Non superare 3 g/die; cautela in caso di epatopatia."
            },
            new SuggerimentoOpzione
            {
                Righe = [new RigaSuggerita { Farmaco = "Ibuprofene 600 mg", Posologia = "1 compressa ogni 12 ore a stomaco pieno", Quantita = 1 }],
                Motivazione = "Alternativa antinfiammatoria non steroidea.",
                Avvertenze = "Cautela in caso di gastropatia, insufficienza renale o terapia anticoagulante."
            }
        ];
    }

    private sealed record MistralChatResponse(List<MistralChoice>? Choices);
    private sealed record MistralChoice(MistralMessage? Message);
    private sealed record MistralMessage(string? Content);
    private sealed record MistralOutput(List<SuggerimentoOpzione>? Opzioni);
}
