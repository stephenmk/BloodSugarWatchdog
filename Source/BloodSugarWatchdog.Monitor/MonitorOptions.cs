// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Monitor;

internal sealed class MonitorOptions
{
    public const string ConfigSectionPath = nameof(MonitorService);

    public required int CooldownMinutes { get; init; }
    public required int RangeMinutes { get; init; }

    public required double LowBgl { get; init; }
    public required string LowCaption { get; init; }

    public required double SpikeSlope { get; init; }
    public required string SpikeCaption { get; init; }

    public required double CrashSlope { get; init; }
    public required string CrashCaption { get; init; }
}
