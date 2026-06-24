// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import.Importers;

internal abstract partial class Importer
{
    protected readonly ILogger<Importer> _logger;
    protected readonly BloodSugarContext _context;

    protected Importer(ILogger<Importer> logger, BloodSugarContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void Import(DirectoryInfo directory)
    {
        Initialize();
        var count = ImportDirectory(directory);
        LogNewEntries(count);
    }

    public void Import(JsonArray array)
    {
        Initialize();

        int count = 0;
        foreach (var node in array)
        {
            if (node is not JsonObject obj)
            {
                LogInvalidNode(node?.GetElementIndex());
                continue;
            }
            count += ImportObject(obj);
        }

        _context.SaveChanges();
        LogNewEntries(count);
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
        Console.Error.WriteLine(file.FullName);
        Dictionary<string, JsonObject> data;
        using (var stream = file.OpenRead())
        {
            data = JsonSerializer.Deserialize<Dictionary<string, JsonObject>>(stream) ?? [];
        }
        int count = 0;
        foreach (var (key, obj) in data)
        {
            count += ImportObject(obj);
        }
        _context.SaveChanges();
        return count;
    }

    private int ImportObject(JsonObject obj)
    {
        int count = 0;
        try
        {
            foreach (var (property, _) in obj)
            {
                if (!KnownProperties.Contains(property))
                    throw new Exception($"Unknown property name `{property}`");
            }
            if (AddObject(obj))
                count++;
        }
        catch (Exception ex)
        {
            LogInvalidObject(ex.Message);
            AddErrorRecord(obj, ex);
        }
        return count;
    }

    private void AddErrorRecord(JsonObject obj, Exception ex)
    {
        var uniqueIdentifier = obj.TryGetPropertyValue("uuid", out var uuid) && uuid is not null
            ? uuid.ToString()
            : obj.TryGetPropertyValue("_id", out var id) && id is not null
            ? id.ToString()
            : null;

        if (uniqueIdentifier is not null && _context.ErrorRecords.Any(r => r.UniqueIdentifier == uniqueIdentifier))
            return;

        _context.ErrorRecords.Add(new ErrorRecord
        {
            Id = default,
            CreatedAt = DateTime.UtcNow,
            UniqueIdentifier = uniqueIdentifier,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            RecordJson = JsonSerializer.SerializeToUtf8Bytes(obj),
        });
    }

    protected abstract bool AddObject(JsonObject obj);
    protected abstract FrozenSet<string> KnownProperties { get; }

    [LoggerMessage(LogLevel.Warning, "JsonArray contains unexpected node type at index `{Index}`")]
    partial void LogInvalidNode(int? index);

    [LoggerMessage(LogLevel.Warning, "Exception occurred while processing object: `{Message}`")]
    partial void LogInvalidObject(string message);

    private void LogNewEntries(int count)
    {
        if (count == 1)
            LogOneNewEntry();
        else
            LogMultipleNewEntries(count);
    }

    protected abstract void LogOneNewEntry();
    protected abstract void LogMultipleNewEntries(int count);
}
