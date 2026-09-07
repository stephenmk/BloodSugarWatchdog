// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using System.Threading.Channels;
using BloodSugarBot.Dto;
using BloodSugarBot.Import;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodSugarBot.Nightscout;

internal sealed partial class NightscoutService
(
    ILogger<NightscoutService> logger,
    NightscoutHttpClient client,
    IImporter<BloodGlucoseEntry> bglImporter,
    IImporter<BloodGlucoseTreatment> treatmentImporter,
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
        ImmutableArray<BloodGlucoseEntry> Entries,
        ImmutableArray<BloodGlucoseTreatment> Treatments
    );

    private async Task<Data> GetDataAsync(CancellationToken ct)
    {
        ImmutableArray<BloodGlucoseEntry> entries = [];
        ImmutableArray<BloodGlucoseTreatment> treatments = [];
        try
        {
            entries = await client.GetEntriesAsync(ct);
            treatments = await client.GetTreatmentsAsync(ct);
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
