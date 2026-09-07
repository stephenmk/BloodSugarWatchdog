// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarBot.Notify;
using BloodSugarBot.Plotting;
using Microsoft.Extensions.Options;

namespace BloodSugarBot.Monitor.Checkers;

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
