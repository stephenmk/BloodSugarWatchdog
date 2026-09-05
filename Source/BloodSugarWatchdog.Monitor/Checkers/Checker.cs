// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;

namespace BloodSugarWatchdog.Monitor.Checkers;

internal abstract class Checker
{
    private readonly INotifyService _notify;
    private readonly IStatusPlotter _plotter;
    private long _lastNotification;

    protected Checker(INotifyService notify, IStatusPlotter plotter) =>
        (_notify, _plotter) =
        (@notify, @plotter);

    public async Task<bool> CheckAsync(ImmutableArray<Bgl> bgls, CancellationToken ct)
    {
        if (IsCooldown())
            return false;

        if (!IsMatch(bgls))
            return false;

        var path = Path.GetTempFileName();
        _plotter.RenderToPath(path);

        var success = await _notify.PostImageAsync(path, GetCaption(bgls), ct);

        if (success)
        {
            _lastNotification = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected abstract bool IsMatch(ImmutableArray<Bgl> bgls);
    protected abstract string GetCaption(ImmutableArray<Bgl> bgls);

    private bool IsCooldown()
        => DateTimeOffset.Now.AddMinutes(-30).ToUnixTimeMilliseconds() < _lastNotification;
}
