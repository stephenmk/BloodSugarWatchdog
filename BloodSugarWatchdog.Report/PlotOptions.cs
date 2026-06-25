// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Report;

public sealed record PlotOptions
{
    public const string ConfigSectionPath = nameof(PlotOptions);

    public required string Username { get; init; }
    public required string TimeZone { get; init; }
    public required double VeryHighBgl { get; init; }
    public required double HighBgl { get; init; }
    public required double LowBgl { get; init; }
    public required double VeryLowBgl { get; init; }
}
