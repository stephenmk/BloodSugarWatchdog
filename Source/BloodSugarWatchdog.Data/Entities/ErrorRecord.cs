// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.EntityFrameworkCore;

namespace BloodSugarWatchdog.Data.Entities;

[Index(nameof(UniqueIdentifier), IsUnique = false)]
public sealed class ErrorRecord
{
    public required int Id { get; init; }
    public required string? UniqueIdentifier { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string Message { get; init; }
    public required string? StackTrace { get; init; }
    public required byte[] RecordJson { get; init; }
}
