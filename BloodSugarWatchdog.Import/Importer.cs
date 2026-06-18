// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;

namespace BloodSugarWatchdog.Import;

public abstract class Importer
{
    protected readonly Context _context;

    protected Importer(Context context)
    {
        _context = context;
    }

    public int Import(DirectoryInfo directory)
    {
        Initialize();
        return ProcessDirectory(directory);
    }

    public int Import(JsonArray array)
    {
        Initialize();

        int count = 0;
        foreach (var node in array)
        {
            if (node is not JsonObject obj)
            {
                Console.Error.WriteLine($"JsonArray contains unexpected node type `{node?.GetElementIndex()}`");
                continue;
            }
            try
            {
                count += ProcessObject(obj);
            }
            catch
            {
                throw;
            }
        }

        _context.SaveChanges();
        return count;
    }

    protected abstract void Initialize();

    private int ProcessDirectory(DirectoryInfo directory)
    {
        int count = 0;
        foreach (var info in directory.GetFileSystemInfos())
        {
            if (info is FileInfo file && file.FullName.EndsWith(".json"))
            {
                count += ProcessFile(file);
            }
            else if (info is DirectoryInfo subdir)
            {
                count += ProcessDirectory(subdir);
            }
        }
        return count;
    }

    private int ProcessFile(FileInfo file)
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
                count += ProcessObject(obj);
            }
            catch
            {
                Console.Error.WriteLine($"Error for key {key} in file {file}");
                throw;
            }
        }
        _context.SaveChanges();
        return count;
    }

    private int ProcessObject(JsonObject obj)
    {
        int count = 0;
        foreach (var (property, _) in obj)
        {
            if (!KnownProperties.Contains(property))
                throw new Exception($"Unknown property name `{property}`");
        }
        if (ProcessObj(obj))
            count++;
        return count;
    }

    protected abstract bool ProcessObj(JsonObject obj);
    protected abstract FrozenSet<string> KnownProperties { get; }
}
