using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DipcClient;

public sealed class DipcApiClient
{
    private readonly HttpClient _httpClient;

    public DipcApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool ok, string message)> SendReportAsync(Uri endpoint, string apiKey, PcReport report, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(report, AppSettings.ReportJsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey.Trim());
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Wysłano OK");
        }

        var msg = string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase ?? "Błąd" : body;
        return (false, msg);
    }
}

