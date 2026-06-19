// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.CommandLine;
using BloodSugarWatchdog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Nightscout;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using var provider = GetServiceProvider(parsedArgs.Username);

        using var context = provider.GetRequiredService<BloodSugarContext>();
        context.Database.Migrate();

        var service = provider.GetRequiredService<INightscoutService>();

        try
        {
            await service.RunAsync(parsedArgs.MillisecondsDelay, cts.Token);
        }
        catch (TaskCanceledException ex)
        {
            Console.Error.WriteLine(ex.Message);
        }

        return 0;
    }

    private sealed record ParsedArgs
    (
        string Username,
        int MillisecondsDelay
    );

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var usernameOption = new Option<string>("--user") { Required = true };
        var pollOption = new Option<int?>("--poll");

        var rootCommand = new RootCommand("Continuously fetch data from Nightscout API")
        {
            usernameOption,
            pollOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
            Console.Error.WriteLine(parseError.Message);

        if (parseResult.Errors.Any())
            return null;

        var username = parseResult.GetRequiredValue(usernameOption);
        var pollRate = parseResult.GetValue(pollOption) ?? 5;

        return new(username, pollRate * 60 * 1000);
    }

    private static ServiceProvider GetServiceProvider(string username)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddNightscoutService(options =>
        {
            options.Username = username;
            options.ClientUserAgent = "BloodSugarWatchdog/1.0";
        });

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
