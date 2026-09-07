// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarBot.Dto;
using BloodSugarBot.Import.Importers;
using Microsoft.Extensions.DependencyInjection;

namespace BloodSugarBot.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportServices(this IServiceCollection services)
        => services
            .AddTransient<IImporter<BloodGlucoseEntry>, BglImporter>()
            .AddTransient<IImporter<BloodGlucoseTreatment>, TreatmentImporter>();
}

public interface IImporter<T>
{
    int Import(DirectoryInfo directory);
    int Import(IEnumerable<T> entries);
}
