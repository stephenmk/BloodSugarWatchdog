// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloodSugarWatchdog.Data;

public sealed class BloodSugarContextFactory : IDesignTimeDbContextFactory<BloodSugarContext>
{
    public BloodSugarContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BloodSugarContext>();

        optionsBuilder.UseSqlite(ApplicationPaths.GetSqliteConnectionString(args[0]));

        return new BloodSugarContext(optionsBuilder.Options);
    }
}
