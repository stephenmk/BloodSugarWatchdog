// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarWatchdog.Data.Entities;

public sealed class TreatmentDevice
{
    public required int Id { get; init; }
    public required string? Name { get; init; }

    public List<Treatment> Treatments { get; init; } = [];
}
