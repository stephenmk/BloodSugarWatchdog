// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.Extensions.Logging;
using ScottPlot;

namespace BloodSugarWatchdog.Report;

internal abstract class Plotter
{
    protected readonly ILogger<Plotter> _logger;
    protected readonly BloodSugarContext _context;
    protected readonly PlotOptions _options;

    protected Plotter(ILogger<Plotter> logger, BloodSugarContext context, PlotOptions options) =>
        (_logger, _context, _options) =
        (@logger, @context, @options);

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

    protected void AddBglData(Plot plot)
    {
        var timeLength = HoursToMilliseconds(Hours + 0.25);
        var timeEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeStart = timeEnd - timeLength;

        var data = _context.BglEntries
            .Where(e => e.Timestamp >= timeStart)
            .Select(e => new
            {
                MillisecondsAgo = e.Timestamp - timeEnd,
                e.MillimolePerLiter,
            });

        double minimumY = 2;
        double maximumY = 16;

        foreach (var datum in data)
        {
            var x = MillisecondsToHours(datum.MillisecondsAgo);
            var y = datum.MillimolePerLiter;

            minimumY = Math.Min(minimumY, y);
            maximumY = Math.Max(maximumY, y);

            var color = y > _options.LowBgl && y < _options.HighBgl
                ? Color.FromColor(System.Drawing.Color.Green)
                : y > _options.VeryLowBgl && y < _options.VeryHighBgl
                ? Color.FromColor(System.Drawing.Color.Orange)
                : Color.FromColor(System.Drawing.Color.Red);

            plot.Add.Marker(x, y, MarkerShape.FilledCircle, size: 10, color);
        }

        plot.Axes.SetLimitsX(-(Hours + 0.1), 0.1);
        plot.Axes.SetLimitsY(minimumY, maximumY);
    }

    private static double HoursToMilliseconds(double hours)
        => hours * 60 * 60 * 1000;

    private static double MillisecondsToHours(double milliseconds)
        => milliseconds / 1000 / 60 / 60;
}
