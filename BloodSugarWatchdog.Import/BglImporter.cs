// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Entities;
using BloodSugarWatchdog.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import;

public interface IBglImporter
{
    void Import(DirectoryInfo directory);
    void Import(JsonArray array);
}

internal sealed partial class BglImporter
(
    ILogger<BglImporter> logger,
    BloodSugarContext context
)
    : Importer(logger, context), IBglImporter
{
    protected override void Initialize()
    {
        _context.BglDevices.Load();
        InitializeDirections();
    }

    private void InitializeDirections()
    {
        _context.BglDirections.Load();
        if (_context.BglDirections.Any())
        {
            return;
        }
        foreach (var type in Enum.GetValues<BglDirectionType>())
        {
            _context.BglDirections.Add(new BglDirection
            {
                Type = type,
                Name = type.ToString(),
            });
        }
    }

    protected override bool AddObject(JsonObject obj)
    {
        var id = (string)obj["_id"]!;

        if (_context.BglEntries.Any(bgl => bgl.Id == id))
            return false;

        _context.BglEntries.Add(new BglEntry
        {
            Id = id,
            Type = (string)obj["type"]!,
            DeviceId = GetDeviceId(obj),
            Sgv = (int)obj["sgv"]!,
            Timestamp = GetTimestamp(obj),
            Delta = (decimal)obj["delta"]!,
            Filtered = (int)obj["filtered"]!,
            Unfiltered = (int)obj["unfiltered"]!,
            Rssi = (int)obj["rssi"]!,
            UtcOffset = (int)obj["utcOffset"]!,
            Noise = (int)obj["noise"]!,
            SysTime = GetSysTime(obj),
            DirectionType = DirectionToDirectionType((string)obj["direction"]!),
        });

        return true;
    }

    private int GetDeviceId(JsonObject obj)
    {
        var name = (string)obj["device"]!;

        if (!_context.BglDevices.Any(d => d.Name == name))
        {
            _context.BglDevices.Add(new BglDevice
            {
                Id = default,
                Name = name,
            });
            _context.SaveChanges();
        }

        return _context.BglDevices
            .Where(d => d.Name == name)
            .First()
            .Id;
    }

    private static long GetTimestamp(JsonObject obj)
    {
        if (obj.ContainsKey("mills") && obj.ContainsKey("date"))
        {
            var mills = (long)(double)obj["mills"]!;
            var date = (long)(double)obj["date"]!;
            if (mills != date)
                throw new Exception("`mills` and `date` values are not equal");
            return mills;
        }
        else if (obj.ContainsKey("mills"))
            return (long)(double)obj["mills"]!;
        else if (obj.ContainsKey("date"))
            return (long)(double)obj["date"]!;
        else
            throw new Exception("No `date` or `mills` property found");
    }

    private static DateTime GetSysTime(JsonObject obj)
    {
        if (obj.ContainsKey("sysTime") && obj.ContainsKey("dateString"))
        {
            var sysTime = DateTime.Parse((string)obj["sysTime"]!).ToUniversalTime();
            var date = DateTime.Parse((string)obj["dateString"]!).ToUniversalTime();
            if (sysTime != date)
                throw new Exception("`sysTime` and `dateString` values are not equal");
            return sysTime;
        }
        else if (obj.ContainsKey("sysTime"))
            return DateTime.Parse((string)obj["sysTime"]!).ToUniversalTime();
        else if (obj.ContainsKey("dateString"))
            return DateTime.Parse((string)obj["dateString"]!).ToUniversalTime();
        else
            throw new Exception("No `sysTime` or `dateString` property found");
    }

    [LoggerMessage(LogLevel.Information, "Downloaded 1 new BGL entry.")]
    protected override partial void LogOneNewEntry();

    [LoggerMessage(LogLevel.Information, "Downloaded {Count:N0} new BGL entries.")]
    protected override partial void LogMultipleNewEntries(int count);

    private static BglDirectionType DirectionToDirectionType(string direction)
        => direction switch
        {
            #pragma warning disable format
            "NONE"              => BglDirectionType.None,
            "DoubleUp"          => BglDirectionType.DoubleUp,
            "SingleUp"          => BglDirectionType.SingleUp,
            "FortyFiveUp"       => BglDirectionType.FortyFiveUp,
            "Flat"              => BglDirectionType.Flat,
            "FortyFiveDown"     => BglDirectionType.FortyFiveDown,
            "SingleDown"        => BglDirectionType.SingleDown,
            "DoubleDown"        => BglDirectionType.DoubleDown,
            "NOT COMPUTABLE"    => BglDirectionType.NotComputable,
            "RATE OUT OF RANGE" => BglDirectionType.RateOutOfRange,
            _                   => throw new ArgumentOutOfRangeException(nameof(direction))
            #pragma warning restore format
        };

    protected override FrozenSet<string> KnownProperties { get; } = new HashSet<string>()
    {
        "_id",
        "date",
        "dateString",
        "delta",
        "device",
        "direction",
        "filtered",
        "mills",
        "noise",
        "rssi",
        "sgv",
        "sysTime",
        "type",
        "unfiltered",
        "utcOffset",
    }
    .ToFrozenSet();
}
