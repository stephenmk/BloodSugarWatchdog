// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarBot.Data;
using BloodSugarBot.Notify;
using BloodSugarBot.Plotting;
using Microsoft.Extensions.Options;

namespace BloodSugarBot.Monitor.Checkers;

internal sealed class SpikeChecker
(
    BloodSugarContext context,
    IOptions<MonitorOptions> options,
    INotifyService notify,
    IStatusPlotter plotter
) :
    Checker(options, notify, plotter)
{
    protected override bool IsMatch(ImmutableArray<Bgl> bgls)
    {
        if (bgls.Last().MillimolePerLiter < _options.Value.SpikeBgl)
            return false;

        if (CalculateSlope(bgls) is not double slope)
            return false;

        if (slope < _options.Value.SpikeSlope)
            return false;

        if (RecentTreatmentExists())
            return false;

        return true;
    }

    private bool RecentTreatmentExists()
    {
        var end = DateTime.UtcNow;
        var start = end.AddHours(-1);

        return context.Treatments
            .Where(static e => e.Carbs != null)
            .Where(e => e.SysTime >= start && e.SysTime <= end)
            .Any();
    }

    protected override string GetCaption(ImmutableArray<Bgl> bgls)
        => _options.Value.SpikeCaption;
}
