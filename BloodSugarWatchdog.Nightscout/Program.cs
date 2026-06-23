// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Nightscout;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var builder = GetAppBuilder(args);

        if (builder is null)
            return 1;

        using var host = builder.Build();
        await host.RunAsync();

        return 0;
    }

    private static HostApplicationBuilder? GetAppBuilder(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return null;

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.SingleLine = false;
        });

        builder.Services.AddNightscoutService(options =>
        {
            options.Username = parsedArgs.Username;
            options.HttpClientUserAgent = "BloodSugarWatchdog/1.0";
        });

        return builder;
    }

    private sealed record ParsedArgs
    (
        string Username
    );

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var usernameOption = new Option<string>("--user") { Required = true };

        var rootCommand = new RootCommand("Continuously fetch data from Nightscout API")
        {
            usernameOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
            Console.Error.WriteLine(parseError.Message);

        if (parseResult.Errors.Any())
            return null;

        var username = parseResult.GetRequiredValue(usernameOption);

        return new(username);
    }
}
