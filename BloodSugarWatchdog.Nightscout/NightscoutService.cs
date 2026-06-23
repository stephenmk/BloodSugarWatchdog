// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Nodes;
using BloodSugarWatchdog.Import.Importers;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Nightscout;

internal sealed partial class NightscoutService
(
    ILogger<NightscoutService> logger,
    NightscoutHttpClient client,
    BglImporter bglImporter,
    TreatmentImporter treatmentImporter
)
    : INightscoutService
{
    public async Task RunAsync(int millisecondsDelay, CancellationToken ct = default)
    {
        while (true)
        {
            var delayTask = Task.Delay(millisecondsDelay, ct);
            var data = await GetDataAsync(ct);

            var bglCount = bglImporter.Import(data.Entries);
            LogNewEntries(bglCount);

            var treatmentCount = treatmentImporter.Import(data.Treatments);
            LogNewTreatments(treatmentCount);

            await delayTask;
        }
    }

    private sealed record Data(JsonArray Entries, JsonArray Treatments);

    private async Task<Data> GetDataAsync(CancellationToken ct)
    {
        JsonArray entries = [];
        JsonArray treatments = [];
        try
        {
            entries = await client.GetEntriesAsync(ct) ?? [];
            treatments = await client.GetTreatmentsAsync(ct) ?? [];
        }
        catch (HttpRequestException ex)
        {
            LogHttpRequestException(ex.Message);
        }
        return new(entries, treatments);
    }

    [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new BGL entries.")]
    partial void LogNewEntries(int count);

    [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new treatment records.")]
    partial void LogNewTreatments(int count);

    [LoggerMessage(LogLevel.Warning, "HttpRequestException: {Message}")]
    partial void LogHttpRequestException(string message);
}
