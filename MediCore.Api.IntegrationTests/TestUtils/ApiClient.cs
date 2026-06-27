using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MediCore.Api.IntegrationTests.TestUtils;

// Wrapper minimo su HttpClient per chiamare il server reale di MediCore.Api in localhost.
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient()
    {
        // Niente riuso di connessioni dal pool: evita IOException intermittenti quando
        // Kestrel chiude una connessione keep-alive che HttpClient tenta di riutilizzare.
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.Zero };
        _http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5095/") };
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }

    public void UseToken(string? token) =>
        _http.DefaultRequestHeaders.Authorization = token is null
            ? null
            : new AuthenticationHeaderValue("Bearer", token);

    public Task<HttpResponseMessage> GetAsync(string url) => _http.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync<T>(string url, T body) => _http.PostAsJsonAsync(url, body);

    public Task<HttpResponseMessage> PutAsync<T>(string url, T body) => _http.PutAsJsonAsync(url, body);

    public async Task<HttpResponseMessage> PostFileAsync(string url, Guid prenotazioneId, byte[] fileContent, string fileName, string contentType, string? contenuto = null)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent(prenotazioneId.ToString()), "PrenotazioneId" }
        };
        var fileParte = new ByteArrayContent(fileContent);
        fileParte.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileParte, "File", fileName);
        if (contenuto is not null)
            form.Add(new StringContent(contenuto), "Contenuto");

        return await _http.PostAsync(url, form);
    }
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, IReadOnlyList<string> Ruoli);
