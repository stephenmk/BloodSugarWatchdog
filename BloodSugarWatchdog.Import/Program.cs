using System.CommandLine;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        Importer importer;
        switch (parsedArgs.DataType)
        {
            case DataType.Bgl:
                importer = new BglImporter();
                importer.Import(parsedArgs.Username, parsedArgs.Directory);
                break;
            case DataType.Treatment:
                importer = new TreatmentImporter();
                importer.Import(parsedArgs.Username, parsedArgs.Directory);
                break;
        }

        return 0;
    }

    private enum DataType
    {
        Bgl,
        Treatment,
    }

    private sealed record ParsedArgs(string Username, DirectoryInfo Directory, DataType DataType);

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
}
