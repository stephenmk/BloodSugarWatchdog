// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Monitor.Checkers;

internal sealed class LowChecker
(
    IOptions<MonitorOptions> options,
    INotifyService notify,
    IStatusPlotter plotter
) :
    Checker(options, notify, plotter)
{
    protected override bool IsMatch(ImmutableArray<Bgl> bgls)
        => bgls.Last().MillimolePerLiter <= _options.Value.LowBgl;

    protected override string GetCaption(ImmutableArray<Bgl> bgls)
        => _options.Value.LowCaption;
}
