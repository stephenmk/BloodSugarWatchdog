// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.ComponentModel.DataAnnotations.Schema;
using BloodSugarWatchdog.Data.Enums;

namespace BloodSugarWatchdog.Data.Entities;

public sealed class BglEntry
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required int DeviceId { get; init; }
    public required int Sgv { get; init; }
    public required decimal Delta { get; init; }
    public required BglDirectionType DirectionType { get; init; }
    public required int Filtered { get; init; }
    public required int Unfiltered { get; init; }
    public required int Rssi { get; init; }
    public required int Noise { get; init; }
    public required long Timestamp { get; init; }
    public required DateTime SysTime { get; init; }
    public required int UtcOffset { get; init; }

    public double MillimolePerLiter => Sgv / 18.018;

    [ForeignKey(nameof(DirectionType))]
    public BglDirection Direction { get; init; } = null!;

    [ForeignKey(nameof(DeviceId))]
    public BglDevice Device { get; init; } = null!;
}
