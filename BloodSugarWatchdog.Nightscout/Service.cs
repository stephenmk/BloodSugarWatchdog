using BloodSugarWatchdog.Import;

namespace BloodSugarWatchdog.Nightscout;

internal sealed partial class Service
(
    NightscoutClient client,
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

            var entries = await client.GetEntriesAsync(ct) ?? [];
            var treatments = await client.GetTreatmentsAsync(ct) ?? [];

            var bglCount = bglImporter.Import(entries);
            Console.Error.WriteLine($"Downloaded {bglCount:N0} new BGL entries.");

            var treatmentCount = treatmentImporter.Import(treatments);
            Console.Error.WriteLine($"Downloaded {treatmentCount:N0} new treatment records.");

            await delayTask;
        }
    }

    // [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new BGL entries.")]
    // partial void LogNewEntries(int count);

    // [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new treatment records.")]
    // partial void LogNewTreatments(int count);
}
