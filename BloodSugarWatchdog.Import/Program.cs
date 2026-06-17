using System.CommandLine;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        var importer = new BglImporter();
        importer.Import(parsedArgs.Username, parsedArgs.Directory);

        return 0;
    }

    private sealed record ParsedArgs(string Username, DirectoryInfo Directory);

    private static ParsedArgs? ParseArgs(string[] args)
    {
        var usernameOption = new Option<string>("--user") { Required = true };
        var dirOption = new Option<DirectoryInfo>("--directory") { Required = true };

        var rootCommand = new RootCommand("Import nightscout data from JSON files")
        {
            usernameOption,
            dirOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
            Console.Error.WriteLine(parseError.Message);

        if (parseResult.Errors.Any())
            return null;

        var username = parseResult.GetRequiredValue(usernameOption);
        var dir = parseResult.GetRequiredValue(dirOption);

        if (!dir.Exists)
        {
            Console.Error.WriteLine($"Directory at path {dir.FullName} does not exist.");
            return null;
        }

        return new(username, dir);
    }
}
