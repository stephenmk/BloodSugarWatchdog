using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;

namespace BloodSugarWatchdog.Import;

public abstract class Importer
{
    public int Import(string username, DirectoryInfo directory)
    {
        using var context = new Context(username);
        context.Database.EnsureCreated();

        Initialize(context);
        var count = ProcessDirectory(context, directory);

        context.SaveChanges();
        return count;
    }

    protected abstract void Initialize(Context context);

    private int ProcessDirectory(Context context, DirectoryInfo directory)
    {
        int count = 0;
        foreach (var info in directory.GetFileSystemInfos())
        {
            if (info is FileInfo file && file.FullName.EndsWith(".json"))
            {
                count += ProcessFile(context, file);
            }
            else if (info is DirectoryInfo subdir)
            {
                count += ProcessDirectory(context, subdir);
            }
        }
        return count;
    }

    private int ProcessFile(Context context, FileInfo file)
    {
        Console.Error.WriteLine(file.FullName);
        Dictionary<string, JsonObject> data;
        using (var stream = file.OpenRead())
        {
            data = JsonSerializer.Deserialize<Dictionary<string, JsonObject>>(stream) ?? [];
        }
        int count = 0;
        foreach (var (key, obj) in data)
        {
            try
            {
                foreach (var (property, _) in obj)
                {
                    if (!KnownProperties.Contains(property))
                        throw new Exception($"Unknown property name `{property}`");
                }
                if (ProcessObj(context, obj))
                    count++;
            }
            catch
            {
                Console.Error.WriteLine($"Error for key {key} in file {file}");
                throw;
            }
        }
        context.SaveChanges();
        return count;
    }

    protected abstract bool ProcessObj(Context context, JsonObject obj);
    protected abstract FrozenSet<string> KnownProperties { get; }
}
