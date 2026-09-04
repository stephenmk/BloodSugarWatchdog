// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Threading.Channels;
using BloodSugarWatchdog.Import;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Nightscout;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNightscoutService(this IServiceCollection services)
    {
        services.AddOptions<NightscoutOptions>()
            .BindConfiguration(NightscoutOptions.ConfigSectionPath)
            .ValidateOnStart();

        services.AddHttpClient<NightscoutHttpClient>(static (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NightscoutOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiEndpoint);
            client.DefaultRequestHeaders.Add("User-Agent", options.HttpClientUserAgent);
        });

        services.AddImportServices();

        services.AddHostedService<NightscoutService>();

        services.AddSingleton(Channel.CreateUnbounded<long>())
            .AddSingleton(static sp => sp.GetRequiredService<Channel<long>>().Reader)
            .AddSingleton(static sp => sp.GetRequiredService<Channel<long>>().Writer);

        return services;
    }
}
