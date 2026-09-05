// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using System.Threading.Channels;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Monitor.Checkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Monitor;

internal readonly record struct Bgl(long Timestamp, double MillimolePerLiter);

internal sealed class MonitorService
(
    IOptions<MonitorOptions> options,
    BloodSugarContext context,
    ChannelReader<long> eventReader,
    CrashChecker crashChecker,
    LowChecker lowChecker,
    SpikeChecker spikeChecker
) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var checkers = new Checker[]
        {
            crashChecker,
            lowChecker,
            spikeChecker,
        };

        await foreach (var _ in eventReader.ReadAllAsync(ct))
        {
            var bgls = GetRecentBgls();

            if (!bgls.Any())
                continue;

            foreach (var checker in checkers)
            {
                if (await checker.CheckAsync(bgls, ct))
                    break;
            }
        }
    }

    private ImmutableArray<Bgl> GetRecentBgls()
    {
        var rangeSize = options.Value.RangeMinutes;
        var now = DateTimeOffset.Now;
        var rangeStart = now.AddMinutes(-rangeSize).ToUnixTimeMilliseconds();
        var rangeEnd = now.ToUnixTimeMilliseconds();
        var query = BglQuery(context, rangeStart, rangeEnd);
        return query.ToImmutableArray();
    }

    private static readonly Func<BloodSugarContext, long, long, IEnumerable<Bgl>> BglQuery = EF.CompileQuery
    (
        static (BloodSugarContext context, long start, long end)
            => context.BglEntries
                .Where(e => e.Timestamp > start && e.Timestamp < end)
                .OrderBy(static e => e.Timestamp)
                .Select(static e => new Bgl(e.Timestamp, e.MillimolePerLiter))
    );
}
