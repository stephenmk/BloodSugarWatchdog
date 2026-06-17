using System.CommandLine;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        Option<string> usernameOption = new("--user")
        {
            Required = true
        };

        Option<DirectoryInfo> dirOption = new("--directory")
        {
            Required = true
        };

        var rootCommand = new RootCommand("Import JMdict XML documents")
        {
            usernameOption,
            dirOption,
        };

        var parseResult = rootCommand.Parse(args);

        foreach (var parseError in parseResult.Errors)
        {
            Console.Error.WriteLine(parseError.Message);
        }

        if (parseResult.Errors.Any())
        {
            return 1;
        }

        var username = parseResult.GetRequiredValue(usernameOption);
        var dir = parseResult.GetRequiredValue(dirOption);

        if (!dir.Exists)
        {
            Console.Error.WriteLine($"Directory at path {dir.FullName} does not exist.");
            return 1;
        }

        var importer = new BglImporter();

        importer.Import(username, dir);

        return 0;
    }
}