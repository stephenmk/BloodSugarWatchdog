// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.ComponentModel.DataAnnotations;
using BloodSugarBot.Data.Enums;

namespace BloodSugarBot.Data.Entities;

public sealed class BglDirection
{
    [Key]
    public required BglDirectionType Type { get; init; }
    public required string Name { get; init; }

    public List<BglEntry> Entries { get; init; } = [];
}
