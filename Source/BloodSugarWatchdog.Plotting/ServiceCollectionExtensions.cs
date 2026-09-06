// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Plotting.Plotters;
using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarWatchdog.Plotting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlottingServices(this IServiceCollection services)
    {
        services.AddOptions<PlotOptions>()
            .BindConfiguration(PlotOptions.ConfigSectionPath)
            .ValidateOnStart();

        return services
            .AddTransient<IStatusPlotter, StatusPlotter>()
            .AddTransient<IDayPlotter, DayPlotter>();
    }
}

public interface IStatusPlotter
{
    void RenderToPath(string path);
}

public interface IDayPlotter
{
    void RenderToPath(string path);
}
