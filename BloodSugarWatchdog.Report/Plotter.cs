// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScottPlot;

namespace BloodSugarWatchdog.Report;

internal abstract partial class Plotter
{
    protected readonly ILogger<Plotter> _logger;
    protected readonly BloodSugarContext _context;
    protected readonly PlotOptions _options;

    protected Plotter(ILogger<Plotter> logger, BloodSugarContext context, IOptions<PlotOptions> options) =>
        (_logger, _context, _options) =
        (@logger, @context, options.Value);

    protected abstract double Hours { get; }

    protected Plot InitializePlot()
    {
        var plot = new Plot();

        var color = Color.FromColor(System.Drawing.Color.Black);
        const float width = 1;

        plot.Add.HorizontalLine(_options.VeryHighBgl, width, color, LinePattern.Dotted);
        plot.Add.HorizontalLine(_options.HighBgl, width, color, LinePattern.Dashed);
        plot.Add.HorizontalLine(_options.LowBgl, width, color, LinePattern.Dashed);
        plot.Add.HorizontalLine(_options.VeryLowBgl, width, color, LinePattern.Dotted);

        plot.Axes.Left.TickGenerator =
            new ScottPlot.TickGenerators.NumericFixedInterval(2);

        return plot;
    }

    protected TimeZoneInfo GetTimeZoneInfo()
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(_options.TimeZone, out var zone))
        {
            zone = TimeZoneInfo.Utc;
            LogUnknownTimezone(_options.TimeZone);
        }
        return zone;
    }

    protected Color GetBglMarkerColor(double y)
        => y > _options.LowBgl && y < _options.HighBgl
            ? Color.FromColor(System.Drawing.Color.Green)
        : y > _options.VeryLowBgl && y < _options.VeryHighBgl
            ? Color.FromColor(System.Drawing.Color.Orange)
        : Color.FromColor(System.Drawing.Color.Red);

    protected static double HoursToMilliseconds(double hours)
        => hours * 60 * 60 * 1000;

    protected static double MillisecondsToHours(double milliseconds)
        => milliseconds / 1000 / 60 / 60;

    [LoggerMessage(LogLevel.Warning, "Unknown timezone {Timezone}.")]
    partial void LogUnknownTimezone(string timezone);
}
