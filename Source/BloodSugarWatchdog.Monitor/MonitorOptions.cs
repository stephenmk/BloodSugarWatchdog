// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Monitor;

internal sealed class MonitorOptions
{
    public const string ConfigSectionPath = nameof(MonitorService);

    public required double LowBgl { get; init; }
    public required string LowCaption { get; init; }

}
