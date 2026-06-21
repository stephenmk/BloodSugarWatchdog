// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.Plottables;

namespace BloodSugarWatchdog.Report;

internal sealed partial class StatusPlotter
(
    ILogger<StatusPlotter> logger,
    BloodSugarContext context,
    PlotOptions options
)
    : IStatusPlotter
{
    private const double hours = 3.0;

    public void RenderToPath(string path)
    {
        using var plot = InitializePlot();

        AddLabels(plot);
        AddBglData(plot);
        AddBolusData(plot);

        plot.SavePng(path, 600, 400);
        LogPlotSaved(path);
    }

    private Plot InitializePlot()
    {
        var plot = new Plot();

        var color = Color.FromColor(System.Drawing.Color.Black);
        const float width = 1;

        plot.Add.HorizontalLine(options.VeryHighBgl, width, color, LinePattern.Dotted);
        plot.Add.HorizontalLine(options.HighBgl, width, color, LinePattern.Dashed);
        plot.Add.HorizontalLine(options.LowBgl, width, color, LinePattern.Dashed);
        plot.Add.HorizontalLine(options.VeryLowBgl, width, color, LinePattern.Dotted);

        plot.Axes.Left.TickGenerator =
            new ScottPlot.TickGenerators.NumericFixedInterval(2);

        return plot;
    }

    private void AddLabels(Plot plot)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out var zone))
        {
            zone = TimeZoneInfo.Utc;
            LogUnknownTimezone(options.TimeZone);
        }

        var zoneNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);

        plot.Title($"{zoneNow:h:mmtt} {zone.StandardName} {zoneNow:(d MMM yyyy)}");
        plot.Axes.Bottom.Label.Text = "hours";
        plot.Axes.Left.Label.Text = "mmol / L";
    }

    private void AddBglData(Plot plot)
    {
        var timeLength = HoursToMilliseconds(hours + 0.25);
        var timeEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeStart = timeEnd - timeLength;

        var data = context.BglEntries
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

            var color = y > options.LowBgl && y < options.HighBgl
                ? Color.FromColor(System.Drawing.Color.Green)
                : y > options.VeryLowBgl && y < options.VeryHighBgl
                ? Color.FromColor(System.Drawing.Color.Orange)
                : Color.FromColor(System.Drawing.Color.Red);

            plot.Add.Marker(x, y, MarkerShape.FilledCircle, size: 10, color);
        }

        plot.Axes.SetLimitsX(-(hours + 0.1), 0.1);
        plot.Axes.SetLimitsY(minimumY, maximumY);
    }

    private void AddBolusData(Plot plot)
    {
        var start = DateTime.UtcNow.AddHours(-hours - 0.25);

        var data = context.Treatments
            .Where(e => e.SysTime >= start)
            .Where(static e => e.Insulin != null)
            .Select(e => new
            {
                X = (e.SysTime - DateTime.UtcNow).TotalHours,
                Insulin = e.Insulin!.Value,
            });

        foreach (var datum in data)
        {
            var line = plot.Add.VerticalLine(datum.X, 1, Color.FromColor(System.Drawing.Color.Blue), LinePattern.Solid);
            line.LabelText = $"{datum.Insulin}U";
            line.LabelOppositeAxis = true;
            line.LabelRotation = -90;
            line.LabelBackgroundColor = Colors.Transparent;
            line.LabelFontColor = Color.FromColor(System.Drawing.Color.Blue);
            line.LabelFontSize = 10;
            line.LabelOffsetY = GetBolusLabelOffsetY(plot, datum.X);
            line.LabelOffsetX = 2;
        }
    }

    private int GetBolusLabelOffsetY(Plot plot, double xCoord)
    {
        var nearestMarker = plot.PlottableList
            .Where(p => p is Marker m && m.Position.X <= xCoord)
            .Select(static p => (Marker)p)
            .OrderByDescending(static m => m.Position.X)
            .FirstOrDefault();

        if (nearestMarker is null || nearestMarker.Y < 12)
            return 50;
        else
            return 200;
    }

    private static double HoursToMilliseconds(double hours)
        => hours * 60 * 60 * 1000;

    private static double MillisecondsToHours(double milliseconds)
        => milliseconds / 1000 / 60 / 60;

    [LoggerMessage(LogLevel.Information, "Plot successfully saved to {Path}.")]
    partial void LogPlotSaved(string path);

    [LoggerMessage(LogLevel.Warning, "Unknown timezone {Timezone}.")]
    partial void LogUnknownTimezone(string timezone);
}
