// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.CommandLine;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Import.Importers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        using var provider = GetServiceProvider(parsedArgs.Username);

        using var context = provider.GetRequiredService<BloodSugarContext>();
        context.Database.Migrate();

        switch (parsedArgs.DataType)
        {
            case DataType.Bgl:
                var bglImporter = provider.GetRequiredService<BglImporter>();
                bglImporter.Import(parsedArgs.Directory);
                break;
            case DataType.Treatment:
                var treatmentImporter = provider.GetRequiredService<TreatmentImporter>();
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
        string Username,
        DirectoryInfo Directory,
        DataType DataType
    );

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var usernameOption = new Option<string>("--user") { Required = true };
        var dirOption = new Option<DirectoryInfo>("--directory") { Required = true };
        var typeOption = new Option<DataType>("--type") { Required = true };

        var rootCommand = new RootCommand("Import nightscout data from JSON files")
        {
            usernameOption,
            dirOption,
            typeOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
            Console.Error.WriteLine(parseError.Message);

        if (parseResult.Errors.Any())
            return null;

        var username = parseResult.GetRequiredValue(usernameOption);
        var dir = parseResult.GetRequiredValue(dirOption);
        var type = parseResult.GetRequiredValue(typeOption);

        if (!dir.Exists)
        {
            Console.Error.WriteLine($"Directory at path {dir.FullName} does not exist.");
            return null;
        }

        return new(username, dir, type);
    }

    private static ServiceProvider GetServiceProvider(string username)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddImportServices(username);

        serviceCollection.AddLogging(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));

        return serviceCollection.BuildServiceProvider();
    }
}
