// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Report;

public sealed class PlotOptions
{
    public string Username { get; set; } = "";
    public string TimeZone { get; set; } = "US/Central";
    public double VeryHighBgl { get; set; } = 14.4;
    public double HighBgl { get; set; } = 10.0;
    public double LowBgl { get; set; } = 3.9;
    public double VeryLowBgl { get; set; } = 3.1;
}
