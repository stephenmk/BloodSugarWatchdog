using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace BloodSugarWatchdog.Nightscout;

internal sealed class NightscoutClient : IDisposable
{
    private readonly string _entriesUrl;
    private readonly string _treatmentsUrl;
    private readonly HttpClient _httpClient;
    private bool _disposedValue;

    public NightscoutClient(string username)
    {
        var domain = $"https://{username}.my.nightscoutpro.com";

        _entriesUrl = $"{domain}/api/v1/entries.json";
        _treatmentsUrl = $"{domain}/api/v1/treatments.json";

        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BloodSugarWatchdog/1.0");
    }

    public Task<JsonArray?> GetEntriesAsync(CancellationToken ct = default)
        => GetContentAsync(_entriesUrl, ct);

    public Task<JsonArray?> GetTreatmentsAsync(CancellationToken ct = default)
        => GetContentAsync(_treatmentsUrl, ct);

    private async Task<JsonArray?> GetContentAsync(string url, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<JsonArray>(ct);

        return content;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}