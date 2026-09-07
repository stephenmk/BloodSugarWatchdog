// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarBot.Notify;
using BloodSugarBot.Plotting;
using Microsoft.Extensions.Options;

namespace BloodSugarBot.Monitor.Checkers;

internal sealed class CrashChecker
(
    IOptions<MonitorOptions> options,
    INotifyService notify,
    IStatusPlotter plotter
) :
    Checker(options, notify, plotter)
{
    protected override bool IsMatch(ImmutableArray<Bgl> bgls)
    {
        if (bgls.Last().MillimolePerLiter > _options.Value.CrashBgl)
            return false;

        if (CalculateSlope(bgls) is not double slope)
            return false;

        if (slope >= 0)
            return false;

        if (Math.Abs(slope) < _options.Value.CrashSlope)
            return false;

        return true;
    }

    protected override string GetCaption(ImmutableArray<Bgl> bgls)
        => _options.Value.CrashCaption;
}
