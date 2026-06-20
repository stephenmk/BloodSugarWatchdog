// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

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
{
    public async Task RunAsync(int millisecondsDelay, CancellationToken ct = default)
    {
        while (true)
        {
            var delayTask = Task.Delay(millisecondsDelay, ct);

            var entries = await client.GetEntriesAsync(ct) ?? [];
            var treatments = await client.GetTreatmentsAsync(ct) ?? [];

            var bglCount = bglImporter.Import(entries);
            LogNewEntries(bglCount);

            var treatmentCount = treatmentImporter.Import(treatments);
            LogNewTreatments(treatmentCount);

            await delayTask;
        }
    }

    [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new BGL entries.")]
    partial void LogNewEntries(int count);

    [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new treatment records.")]
    partial void LogNewTreatments(int count);
}
