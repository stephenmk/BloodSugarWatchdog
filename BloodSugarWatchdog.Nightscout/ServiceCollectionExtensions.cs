// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Threading.Channels;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Paths;
using BloodSugarWatchdog.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Nightscout;

public readonly record struct NewDataEvent(long TimestampMilliseconds);

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNightscoutService(this IServiceCollection services)
    {
        services.AddOptions<NightscoutOptions>()
            .BindConfiguration(NightscoutOptions.ConfigSectionPath)
            .ValidateOnStart();

        return services
            .AddDbContext<BloodSugarContext>(static (sp, options) =>
            {
                var username = sp.GetRequiredService<IOptions<NightscoutOptions>>().Value.Username;
                var connectionString = ApplicationPaths.GetSqliteConnectionString(username);
                options.UseSqlite(connectionString);
            })

            .AddImportServices()

            .AddTransient<NightscoutHttpClient>()
            .AddHostedService<NightscoutService>()

            .AddSingleton(Channel.CreateUnbounded<NewDataEvent>())
            .AddSingleton(static sp => sp.GetRequiredService<Channel<NewDataEvent>>().Reader)
            .AddSingleton(static sp => sp.GetRequiredService<Channel<NewDataEvent>>().Writer);
    }
}
