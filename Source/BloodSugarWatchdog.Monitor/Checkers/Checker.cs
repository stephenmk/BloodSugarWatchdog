// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Monitor.Checkers;

internal abstract class Checker
{
    protected readonly IOptions<MonitorOptions> _options;
    private readonly INotifyService _notify;
    private readonly IStatusPlotter _plotter;
    private long _lastNotification;

    protected Checker(IOptions<MonitorOptions> options, INotifyService notify, IStatusPlotter plotter) =>
        (_options, _notify, _plotter) =
        (@options, @notify, @plotter);

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
    {
        var cooldownMinutes = _options.Value.CooldownMinutes;

        var cooldownEnd = DateTimeOffset.Now
            .AddMinutes(-cooldownMinutes)
            .ToUnixTimeMilliseconds();

        return cooldownEnd < _lastNotification;
    }

    protected static double? CalculateSlope(ImmutableArray<Bgl> bgls)
    {
        if (bgls.Length < 2)
            return null;

        var t0 = bgls[0].Timestamp;
        var x = bgls.Select(r => (r.Timestamp - t0) / 60000.0).ToArray();
        var y = bgls.Select(static r => r.MillimolePerLiter).ToArray();

        int n = x.Length;

        if (n == 2)
        {
            // slope = (y1 - y0) / (x1 - x0)
            var dt = x[1] - x[0];
            return dt == 0
                ? null
                : (y[1] - y[0]) / dt;
        }

        // Least-squares regression: slope = (nΣxy - ΣxΣy) / (nΣx² - (Σx)²)
        double sumX = x.Sum();
        double sumY = y.Sum();
        double sumXY = 0;
        double sumXX = 0;

        for (int i = 0; i < n; i++)
        {
            sumXY += x[i] * y[i];
            sumXX += x[i] * x[i];
        }

        var denominator = (n * sumXX) - (sumX * sumX);

        return denominator == 0
            ? null
            : ((n * sumXY) - (sumX * sumY)) / denominator;
    }
}
