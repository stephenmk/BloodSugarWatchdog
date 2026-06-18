// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportServices(this IServiceCollection services, string username)
    {
        return services
            .AddDbContext<BloodSugarContext>(options =>
            {
                options.UseSqlite(ApplicationPaths.GetSqliteConnectionString(username));
                // Disable EntityFramework logging
                options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddFilter(_ => false); }));
            })
            .AddTransient<BglImporter>()
            .AddTransient<TreatmentImporter>()
            .AddLogging();
    }
}
