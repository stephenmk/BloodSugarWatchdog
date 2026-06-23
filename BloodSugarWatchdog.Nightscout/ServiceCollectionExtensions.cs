// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Import;
using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarWatchdog.Nightscout;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNightscoutService(this IServiceCollection services, Action<NightscoutOptions> configure)
    {
        var serviceOptions = new NightscoutOptions();
        configure(serviceOptions);

        return services
            .AddImportServices(serviceOptions.Username)
            .AddTransient(_ => serviceOptions)
            .AddTransient<NightscoutHttpClient>()
            .AddHostedService<NightscoutService>();
    }
}
