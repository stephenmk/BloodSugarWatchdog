// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Nodes;
using System.Threading.Channels;
using BloodSugarWatchdog.Import;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Nightscout;

internal sealed partial class NightscoutService
(
    ILogger<NightscoutService> logger,
    NightscoutHttpClient client,
    IBglImporter bglImporter,
    ITreatmentImporter treatmentImporter,
    ChannelWriter<long> eventWriter
) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        const int delay = 1000 * 60;
        while (!ct.IsCancellationRequested)
        {
            var data = await GetDataAsync(ct);

            var count = bglImporter.Import(data.Entries);
            treatmentImporter.Import(data.Treatments);

            if (count > 0)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await eventWriter.WriteAsync(now, ct);
            }

            await Task.Delay(delay, ct);
        }
    }

    private sealed record Data
    (
        JsonArray Entries,
        JsonArray Treatments
    );

    private async Task<Data> GetDataAsync(CancellationToken ct)
    {
        JsonArray entries = [];
        JsonArray treatments = [];
        try
        {
            entries = await client.GetEntriesAsync(ct) ?? [];
            treatments = await client.GetTreatmentsAsync(ct) ?? [];
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            LogTimeoutException(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            LogHttpRequestException(ex.Message);
        }
        return new(entries, treatments);
    }

    [LoggerMessage(LogLevel.Warning, "HttpRequestException: {Message}")]
    partial void LogHttpRequestException(string message);

    [LoggerMessage(LogLevel.Warning, "Timeout: {Message}")]
    partial void LogTimeoutException(string message);
}
