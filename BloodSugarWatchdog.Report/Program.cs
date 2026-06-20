// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Paths;
using ScottPlot;

namespace BloodSugarWatchdog.Report;

internal static class Program
{
    private static int Main(string[] args)
    {
        const double hours = 3.0;
        var options = new PlotOptions() { Username = args[0] };

        var factory = new BloodSugarContextFactory();
        using var context = factory.CreateDbContext([options.Username]);

        // Blood sugar entries
        var msLength = HoursToMilliseconds(hours + 0.25);
        var msEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var msStart = msEnd - msLength;

        var bglData = context.BglEntries
            .Where(e => e.Timestamp >= msStart)
            .Select(e => new
            {
                Timestamp = (double)(e.Timestamp - msEnd) / 1000 / 60 / 60,
                e.MillimolePerLiter,
            });

        var dataX = bglData.Select(static e => e.Timestamp).ToArray();
        var dataY = bglData.Select(static e => e.MillimolePerLiter).ToArray();

        var minimumY = dataY.Length == 0 ? 2 : Math.Min(2, dataY.Min());
        var maximumY = dataY.Length == 0 ? 24 : Math.Max(16, dataY.Max());

        // Bolus entries
        var start = DateTime.UtcNow.AddHours(-hours - 0.25);
        var bolusData = context.Treatments
            .Where(e => e.SysTime >= start)
            .Where(static e => e.Insulin != null)
            .Select(e => new
            {
                X = (e.SysTime - DateTime.UtcNow).TotalHours,
                Insulin = e.Insulin!.Value,
            });

        // Setup plot
        using var plot = new Plot();

        var veryHighLine = plot.Add.HorizontalLine(options.VeryHighBgl, 1, Color.FromColor(System.Drawing.Color.Black), LinePattern.Dashed);
        var highLine = plot.Add.HorizontalLine(options.HighBgl, 1, Color.FromColor(System.Drawing.Color.Black), LinePattern.Dotted);
        var lowLine = plot.Add.HorizontalLine(options.LowBgl, 1, Color.FromColor(System.Drawing.Color.Black), LinePattern.Dotted);
        var veryLowLine = plot.Add.HorizontalLine(options.VeryLowBgl, 1, Color.FromColor(System.Drawing.Color.Black), LinePattern.Dashed);

        plot.Title($"{DateTimeOffset.Now:h:mm tt (d MMM yyyy)}");

        plot.Axes.SetLimitsX(-(hours + 0.1), 0.1);
        plot.Axes.SetLimitsY(minimumY, maximumY);

        plot.Axes.Bottom.Label.Text = "hours";
        plot.Axes.Left.Label.Text = "mmol / L";
        plot.Axes.Left.TickGenerator =
            new ScottPlot.TickGenerators.NumericFixedInterval(2);

        // Add data
        foreach (var datum in bglData)
        {
            var x = datum.Timestamp;
            var y = datum.MillimolePerLiter;
            var color = y > options.LowBgl && y < options.HighBgl
                ? Color.FromColor(System.Drawing.Color.Green)
                : y > options.VeryLowBgl && y < options.VeryHighBgl
                ? Color.FromColor(System.Drawing.Color.Orange)
                : Color.FromColor(System.Drawing.Color.Red);

            plot.Add.Marker(x, y, MarkerShape.FilledCircle, size: 10, color);
        }

        foreach (var datum in bolusData)
        {
            var line = plot.Add.VerticalLine(datum.X, 1, Color.FromColor(System.Drawing.Color.Blue), LinePattern.Solid);
            line.LabelText = $"{datum.Insulin}U";
            line.LabelOppositeAxis = true;
            line.LabelRotation = -90;
            line.LabelBackgroundColor = Colors.Transparent;
            line.LabelFontColor = Color.FromColor(System.Drawing.Color.Blue);
            line.LabelFontSize = 10;
            line.LabelOffsetY = 22;
            line.LabelOffsetX = 2;
        }

        var path = Path.Join(ApplicationPaths.GetAppCacheDirPath(options.Username), "plot.png");
        plot.SavePng(path, 600, 400);
        Console.Error.WriteLine($"Plot successfully saved to {path}");

        return 0;
    }

    private static double HoursToMilliseconds(double hours)
        => hours * 60 * 60 * 1000;
}
