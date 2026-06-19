// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data.Paths;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloodSugarWatchdog.Data;

/// <summary>
/// This class is only needed to produce new migration files.
/// E.g. `dotnet ef migrations add NewMigration -- MyUserName`
/// </summary>
public sealed class BloodSugarContextFactory : IDesignTimeDbContextFactory<BloodSugarContext>
{
    public BloodSugarContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BloodSugarContext>();

        optionsBuilder.UseSqlite(ApplicationPaths.GetSqliteConnectionString(args[0]));

        return new BloodSugarContext(optionsBuilder.Options);
    }
}
