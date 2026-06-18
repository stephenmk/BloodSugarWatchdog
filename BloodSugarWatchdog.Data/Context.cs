// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodSugarWatchdog.Data;

public sealed class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Bgl> Bgls { get; set; } = null!;
    public DbSet<BglDirection> BglDirections { get; set; } = null!;
    public DbSet<BglDevice> BglDevices { get; set; } = null!;

    public DbSet<Treatment> Treatments { get; set; } = null!;
    public DbSet<TreatmentDevice> TreatmentDevices { get; set; } = null!;
}
