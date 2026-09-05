// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.CommandLine;
using BloodSugarWatchdog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        using var host = GetHost();

        using var context = host.Services.GetRequiredService<BloodSugarContext>();
        context.Database.Migrate();

        switch (parsedArgs.DataType)
        {
            case DataType.Bgl:
                var bglImporter = host.Services.GetRequiredService<IBglImporter>();
                bglImporter.Import(parsedArgs.Directory);
                break;
            case DataType.Treatment:
                var treatmentImporter = host.Services.GetRequiredService<ITreatmentImporter>();
                treatmentImporter.Import(parsedArgs.Directory);
                break;
        }

        return 0;
    }

    private enum DataType
    {
        Bgl,
        Treatment,
    }

    private sealed record ParsedArgs
    (
        DirectoryInfo Directory,
        DataType DataType
    );

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var dirOption = new Option<DirectoryInfo>("--directory") { Required = true };
        var typeOption = new Option<DataType>("--type") { Required = true };

        var rootCommand = new RootCommand("Import nightscout data from JSON files")
        {
            dirOption,
            typeOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
            Console.Error.WriteLine(parseError.Message);

        if (parseResult.Errors.Any())
            return null;

        var dir = parseResult.GetRequiredValue(dirOption);
        var type = parseResult.GetRequiredValue(typeOption);

        if (!dir.Exists)
        {
            Console.Error.WriteLine($"Directory at path {dir.FullName} does not exist.");
            return null;
        }

        return new(dir, type);
    }

    private static IHost GetHost()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services
            .AddBloodSugarContext()
            .AddImportServices();

        builder.Logging.AddSimpleConsole(static options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        return builder.Build();
    }
}
