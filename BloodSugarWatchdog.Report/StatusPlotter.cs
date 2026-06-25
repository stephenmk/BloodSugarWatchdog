// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScottPlot;
using ScottPlot.Plottables;

namespace BloodSugarWatchdog.Report;

public interface IStatusPlotter
{
    void RenderToPath(string path);
}

internal sealed partial class StatusPlotter : Plotter, IStatusPlotter
{
    public StatusPlotter(ILogger<StatusPlotter> logger, BloodSugarContext context, IOptions<PlotOptions> options)
        : base(logger, context, options) { }

    protected override double Hours => 3.0;

    public void RenderToPath(string path)
    {
        using var plot = InitializePlot();

        AddLabels(plot);
        AddBglData(plot);
        AddBolusData(plot);

        plot.SavePng(path, 600, 400);
        LogPlotSaved(path);
    }

    private void AddLabels(Plot plot)
    {
        var zone = GetTimeZoneInfo();
        var zoneNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);

        plot.Title($"{zoneNow:h:mmtt} {zone.StandardName} {zoneNow:(d MMM yyyy)}");
        plot.Axes.Bottom.Label.Text = "hours";
        plot.Axes.Left.Label.Text = "mmol / L";
    }

    private void AddBglData(Plot plot)
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

            var color = GetBglMarkerColor(y);
            plot.Add.Marker(x, y, MarkerShape.FilledCircle, size: 10, color);
        }

        plot.Axes.SetLimitsX(-(Hours + 0.1), 0.1);
        plot.Axes.SetLimitsY(minimumY, maximumY);
    }

    private void AddBolusData(Plot plot)
    {
        var start = DateTime.UtcNow.AddHours(-(Hours + 0.25));

        var data = _context.Treatments
            .Where(e => e.SysTime >= start)
            .Where(static e => e.Insulin != null)
            .Select(e => new
            {
                X = (e.SysTime - DateTime.UtcNow).TotalHours,
                Insulin = e.Insulin!.Value,
            });

        foreach (var datum in data)
        {
            var line = plot.Add.VerticalLine(
                datum.X,
                width: 1,
                Color.FromColor(System.Drawing.Color.Blue),
                LinePattern.Solid);

            line.LabelText = $"{datum.Insulin}U";
            line.LabelOppositeAxis = true;
            line.LabelRotation = -90;
            line.LabelBackgroundColor = Colors.Transparent;
            line.LabelFontColor = Color.FromColor(System.Drawing.Color.Blue);
            line.LabelFontSize = 10;
            line.LabelOffsetY = GetBolusLabelOffsetY(plot, datum.X);
            line.LabelOffsetX = 2;

            plot.MoveToBottom(line);
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

    [LoggerMessage(LogLevel.Information, "Plot successfully saved to {Path}.")]
    partial void LogPlotSaved(string path);
}
