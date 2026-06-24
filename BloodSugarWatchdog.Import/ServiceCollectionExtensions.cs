// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarWatchdog.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportServices(this IServiceCollection services)
        => services
            .AddTransient<IBglImporter, BglImporter>()
            .AddTransient<ITreatmentImporter, TreatmentImporter>();
}
