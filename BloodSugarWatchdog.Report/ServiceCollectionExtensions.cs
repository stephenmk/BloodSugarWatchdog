// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Paths;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Report;

public interface IStatusPlotter
{
    void RenderToPath(string path);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportService(this IServiceCollection services, Action<PlotOptions> configure)
    {
        var serviceOptions = new PlotOptions();
        configure(serviceOptions);

        return services
            .AddDbContext<BloodSugarContext>(options =>
            {
                options.UseSqlite(ApplicationPaths.GetSqliteConnectionString(serviceOptions.Username));
                // Disable EntityFramework logging
                options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddFilter(_ => false); }));
            })
            .AddLogging()
            .AddTransient(_ => serviceOptions)
            .AddTransient<IStatusPlotter, StatusPlotter>();
    }
}
