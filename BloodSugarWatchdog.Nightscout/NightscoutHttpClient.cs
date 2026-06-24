// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Nightscout;

internal sealed class NightscoutHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposedValue;

    public NightscoutHttpClient(IOptions<NightscoutOptions> options)
    {
        _httpClient = new()
        {
            BaseAddress = new Uri(options.Value.ApiEndpoint),
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", options.Value.HttpClientUserAgent);
    }

    public Task<JsonArray?> GetEntriesAsync(CancellationToken ct)
        => GetContentAsync("entries.json", ct);

    public Task<JsonArray?> GetTreatmentsAsync(CancellationToken ct)
        => GetContentAsync("treatments.json", ct);

    private Task<JsonArray?> GetContentAsync(string requestUri, CancellationToken ct)
        => _httpClient.GetFromJsonAsync<JsonArray>(requestUri, ct);

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                _httpClient.Dispose();
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
