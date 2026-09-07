// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarBot.Data.Paths;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BloodSugarBot.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBloodSugarContext(this IServiceCollection services)
    {
        services.AddOptions<DataOptions>()
            .BindConfiguration(DataOptions.ConfigSectionPath)
            .ValidateOnStart();

        services.AddDbContext<BloodSugarContext>(static (sp, options) =>
        {
            var username = sp.GetRequiredService<IOptions<DataOptions>>().Value.Username;
            var connectionString = ApplicationPaths.GetSqliteConnectionString(username);
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
