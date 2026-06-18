using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

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
        var service = provider.GetRequiredService<IService>();
        await service.RunAsync(parsedArgs.MillisecondsDelay, cts.Token);

        return 0;
    }

    private sealed record ParsedArgs(string Username, int MillisecondsDelay);

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var usernameOption = new Option<string>("--user") { Required = true };
        var pollOption = new Option<int>("--poll") { Required = true };

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
        var pollRate = parseResult.GetRequiredValue(pollOption);

        return new(username, pollRate * 60 * 1000);
    }

    private static ServiceProvider GetServiceProvider(string username)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddNightscoutService(options =>
        {
            options.Username = username;
        });

        return serviceCollection.BuildServiceProvider();
    }
}