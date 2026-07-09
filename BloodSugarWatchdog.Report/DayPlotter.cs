// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScottPlot;
using ScottPlot.Plottables;

namespace BloodSugarWatchdog.Report;

public interface IDayPlotter
{
    void RenderToPath(string path);
}

internal sealed partial class DayPlotter : Plotter, IDayPlotter
{
    public DayPlotter(ILogger<DayPlotter> logger, BloodSugarContext context, IOptions<PlotOptions> options)
        : base(logger, context, options) { }

    protected override double Hours => 24.0;

    public void RenderToPath(string path)
    {
        using var plot = InitializePlot();

        var axis = plot.Axes.DateTimeTicksBottom();
        var tickGen = (ScottPlot.TickGenerators.DateTimeAutomatic)axis.TickGenerator;
        tickGen.LabelFormatter = static dt => dt.ToString("h tt");

        plot.Axes.Left.TickGenerator =
            new ScottPlot.TickGenerators.NumericFixedInterval(2);

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
        var zoneYesterday = zoneNow.AddDays(-1);

        plot.Title($"{zoneYesterday:d MMMM yyyy (dddd)}");
        plot.Axes.Bottom.Label.Text = zone.StandardName.ToLower();
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
                e.Timestamp,
                e.MillimolePerLiter,
            });

        double minimumY = 2;
        double maximumY = 16;

        var zone = GetTimeZoneInfo();

        foreach (var datum in data)
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(datum.Timestamp);
            var x = TimeZoneInfo.ConvertTime(dt, zone).DateTime;
            var y = datum.MillimolePerLiter;

            minimumY = Math.Min(minimumY, y);
            maximumY = Math.Max(maximumY, y);

            var color = GetBglMarkerColor(y);

            plot.Add.Marker(x.ToOADate(), y, MarkerShape.FilledCircle, size: 10, color);
        }

        var now = TimeZoneInfo.ConvertTime(DateTime.Now, zone);
        var leftLimit = now.AddHours(-(Hours + 0.1));
        var rightLimit = now.AddHours(0.1);

        plot.Axes.SetLimitsX(leftLimit.ToOADate(), rightLimit.ToOADate());
        plot.Axes.SetLimitsY(minimumY, maximumY);
    }

    private void AddBolusData(Plot plot)
    {
        var start = DateTime.UtcNow.AddHours(-Hours);

        var data = _context.Treatments
            .Where(e => e.SysTime >= start)
            .Where(static e => e.Carbs != null)
            .Select(e => new
            {
                e.SysTime,
                Carbs = e.Carbs!.Value,
            });

        var zone = GetTimeZoneInfo();

        foreach (var datum in data)
        {
            var dt = DateTime.SpecifyKind(datum.SysTime, DateTimeKind.Utc);
            var x = TimeZoneInfo.ConvertTime(dt, zone);

            var line = plot.Add.VerticalLine(
                x.ToOADate(),
                width: 1,
                Color.FromColor(System.Drawing.Color.Blue),
                LinePattern.Solid);

            line.LabelText = $"{datum.Carbs} carbs";
            line.LabelOppositeAxis = true;
            line.LabelRotation = -90;
            line.LabelBackgroundColor = Colors.Transparent;
            line.LabelFontColor = Color.FromColor(System.Drawing.Color.Blue);
            line.LabelFontSize = 14;
            line.LabelOffsetY = GetBolusLabelOffsetY(plot, x.ToOADate());
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
