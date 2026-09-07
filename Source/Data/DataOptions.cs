// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

namespace BloodSugarBot.Data;

public sealed record DataOptions
{
    public const string ConfigSectionPath = "Database";
    public required string Username { get; init; }
}
