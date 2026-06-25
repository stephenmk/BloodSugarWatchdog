// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarWatchdog.Report;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportService(this IServiceCollection services)
    {
        services.AddOptions<PlotOptions>()
            .BindConfiguration(PlotOptions.ConfigSectionPath)
            .ValidateOnStart();

        return services
            .AddTransient<IStatusPlotter, StatusPlotter>()
            .AddTransient<IDayPlotter, DayPlotter>();
    }
}
