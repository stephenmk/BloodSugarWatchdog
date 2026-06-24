// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Nodes;
using System.Threading.Channels;
using BloodSugarWatchdog.Data;
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
    BloodSugarContext context,
    ChannelWriter<NewDataEvent> channel
)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var data = await GetDataAsync(ct);

            var count = bglImporter.Import(data.Entries);
            treatmentImporter.Import(data.Treatments);

            if (count > 0)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await channel.WriteAsync(new(now), ct);
            }

            var millisecondsDelay = GetMillisecondsDelay();
            await Task.Delay(millisecondsDelay, ct);
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

    private int GetMillisecondsDelay()
    {
        var latestTimestamp = context.BglEntries
            .OrderByDescending(static e => e.Timestamp)
            .Select(static e => e.Timestamp)
            .FirstOrDefault();

        const int defaultDelay = 1 * 60 * 1000; // One minute in milliseconds.

        if (latestTimestamp is default(long)) // No entries in database?
            return defaultDelay;

        var expectedNextEntryTime = DateTimeOffset
            .FromUnixTimeMilliseconds(latestTimestamp)
            .ToUniversalTime()
            .AddMinutes(7); // Two minute gap between entry time and API posting time.

        var delay = (expectedNextEntryTime - DateTime.UtcNow).TotalMilliseconds;

        if (delay < 0)
            delay = defaultDelay;

        LogNextExpectedEntry(DateTime.Now.AddMilliseconds(delay));
        return (int)delay;
    }

    [LoggerMessage(LogLevel.Warning, "HttpRequestException: {Message}")]
    partial void LogHttpRequestException(string message);

    [LoggerMessage(LogLevel.Information, "Next new entries expected at {DateTime:HH:mm:ss}")]
    partial void LogNextExpectedEntry(DateTime dateTime);
}
