// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace BloodSugarBot.Nightscout;

internal sealed class NightscoutHttpClient(HttpClient httpClient)
{
    public Task<JsonArray?> GetEntriesAsync(CancellationToken ct)
        => GetContentAsync("entries.json", ct);

    public Task<JsonArray?> GetTreatmentsAsync(CancellationToken ct)
        => GetContentAsync("treatments.json", ct);

    private Task<JsonArray?> GetContentAsync(string requestUri, CancellationToken ct)
        => httpClient.GetFromJsonAsync<JsonArray>(requestUri, ct);
}
