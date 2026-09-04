// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BloodSugarWatchdog.Notify;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyService(this IServiceCollection services)
    {
        services.AddOptions<NotifyOptions>()
            .BindConfiguration(NotifyOptions.ConfigSectionPath)
            .ValidateOnStart();

        services.AddLogging();

        services.AddHttpClient<INotifyService, NotifyService>(static (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NotifyOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiEndpoint);
            client.DefaultRequestHeaders.Add("User-Agent", options.HttpClientUserAgent);
        });

        return services;
    }
}
