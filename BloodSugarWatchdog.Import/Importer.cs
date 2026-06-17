using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;

namespace BloodSugarWatchdog.Import;

public abstract class Importer
{
    public void Import(string username, DirectoryInfo directory)
    {
        using var context = new Context(username);
        context.Database.EnsureCreated();

        Initialize(context);
        ProcessDirectory(context, directory);

        context.SaveChanges();
    }

    protected abstract void Initialize(Context context);

    private void ProcessDirectory(Context context, DirectoryInfo directory)
    {
        foreach (var info in directory.GetFileSystemInfos())
        {
            if (info is FileInfo file && file.FullName.EndsWith(".json"))
            {
                ProcessFile(context, file);
            }
            else if (info is DirectoryInfo subdir)
            {
                ProcessDirectory(context, subdir);
            }
        }
    }

    private void ProcessFile(Context context, FileInfo file)
    {
        Console.Error.WriteLine(file.FullName);
        Dictionary<string, JsonObject> data;
        using (var stream = file.OpenRead())
        {
            data = JsonSerializer.Deserialize<Dictionary<string, JsonObject>>(stream) ?? [];
        }
        foreach (var (key, obj) in data)
        {
            try
            {
                foreach (var (property, _) in obj)
                {
                    if (!KnownProperties.Contains(property))
                        throw new Exception($"Unknown property name `{property}`");
                }
                ProcessObj(context, obj);
            }
            catch
            {
                Console.Error.WriteLine($"Error for key {key} in file {file}");
                throw;
            }
        }
        context.SaveChanges();
    }

    protected abstract void ProcessObj(Context context, JsonObject obj);
    protected abstract FrozenSet<string> KnownProperties { get; }
}
