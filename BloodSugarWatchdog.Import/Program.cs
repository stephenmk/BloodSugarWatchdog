using System.CommandLine;
using BloodSugarWatchdog.Data;

namespace BloodSugarWatchdog.Import;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);

        if (parsedArgs is null)
            return 1;

        using var context = new Context(parsedArgs.Username);
        context.Database.EnsureCreated();

        int count = 0;

        switch (parsedArgs.DataType)
        {
            case DataType.Bgl:
                var bglImporter = new BglImporter(context);
                count = bglImporter.Import(parsedArgs.Directory);
                break;
            case DataType.Treatment:
                var treatmentImporter = new TreatmentImporter(context);
                count = treatmentImporter.Import(parsedArgs.Directory);
                break;
        }

        Console.Error.WriteLine($"Imported {count:N0} new records.");
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
}
