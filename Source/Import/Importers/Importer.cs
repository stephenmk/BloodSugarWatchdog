// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json;
using BloodSugarBot.Data;
using Microsoft.Extensions.Logging;

namespace BloodSugarBot.Import.Importers;

internal abstract partial class Importer<T> : IImporter<T>
{
    protected readonly ILogger<Importer<T>> _logger;
    protected readonly BloodSugarContext _context;

    protected Importer(ILogger<Importer<T>> logger, BloodSugarContext context)
    {
        _logger = logger;
        _context = context;
    }

    public int Import(DirectoryInfo directory)
    {
        int count;
        using (var transaction = _context.Database.BeginTransaction())
        {
            Initialize();
            transaction.Commit();
        }
        count = ImportDirectory(directory);
        LogNewEntries(count);
        return count;
    }

    public int Import(IEnumerable<T> objs)
    {
        int count = 0;
        using (var transaction = _context.Database.BeginTransaction())
        {
            Initialize();
            foreach (var obj in objs)
            {
                count += ImportObject(obj);
            }
            transaction.Commit();
        }
        LogNewEntries(count);
        return count;
    }

    protected abstract void Initialize();

    private int ImportDirectory(DirectoryInfo directory)
    {
        int count = 0;
        foreach (var info in directory.GetFileSystemInfos())
        {
            if (info is FileInfo file && file.FullName.EndsWith(".json"))
            {
                count += ImportFile(file);
            }
            else if (info is DirectoryInfo subdir)
            {
                count += ImportDirectory(subdir);
            }
        }
        return count;
    }

    private int ImportFile(FileInfo file)
    {
        using var transaction = _context.Database.BeginTransaction();
        Console.Error.WriteLine(file.FullName);
        Dictionary<string, T> data;

        using (var stream = file.OpenRead())
            data = JsonSerializer.Deserialize<Dictionary<string, T>>(stream) ?? [];

        int count = 0;

        foreach (var (key, obj) in data)
        {
            count += ImportObject(obj);
        }

        transaction.Commit();

        return count;
    }

    private int ImportObject(T obj)
    {
        int count = 0;
        try
        {
            if (AddObject(obj))
                count++;
        }
        catch (Exception ex)
        {
            LogInvalidObject(ex.Message);
        }
        return count;
    }

    protected abstract bool AddObject(T obj);

    [LoggerMessage(LogLevel.Warning, "Exception occurred while processing object: `{Message}`")]
    partial void LogInvalidObject(string message);

    private void LogNewEntries(int count)
    {
        if (count == 1)
            LogOneNewEntry();
        else if (count > 1)
            LogMultipleNewEntries(count);
    }

    protected abstract void LogOneNewEntry();
    protected abstract void LogMultipleNewEntries(int count);
}
