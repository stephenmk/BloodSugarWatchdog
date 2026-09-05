// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Nodes;
using BloodSugarWatchdog.Import.Importers;
using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarWatchdog.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportServices(this IServiceCollection services)
        => services
            .AddTransient<IBglImporter, BglImporter>()
            .AddTransient<ITreatmentImporter, TreatmentImporter>();
}

public interface IBglImporter
{
    int Import(DirectoryInfo directory);
    int Import(JsonArray array);
}

public interface ITreatmentImporter
{
    int Import(DirectoryInfo directory);
    int Import(JsonArray array);
}
