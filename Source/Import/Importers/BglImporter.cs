// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarBot.Data;
using BloodSugarBot.Data.Entities;
using BloodSugarBot.Data.Enums;
using BloodSugarBot.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodSugarBot.Import.Importers;

internal sealed partial class BglImporter
(
    ILogger<BglImporter> logger,
    BloodSugarContext context
) :
    Importer<BloodGlucoseEntry>(logger, context)
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
            return;

        foreach (var type in Enum.GetValues<BglDirectionType>())
        {
            _context.BglDirections.Add(new()
            {
                Type = type,
                Name = type.ToString(),
            });
        }

        _context.SaveChanges();
    }

    protected override bool AddObject(BloodGlucoseEntry obj)
    {
        if (_context.BglEntries.Any(bgl => bgl.Id == obj.Id))
            return false;

        _context.BglEntries.Add(new BglEntry
        {
            Id = obj.Id,
            Type = obj.Type,
            DeviceId = GetDeviceId(obj),
            Sgv = obj.Sgv,
            Timestamp = GetTimestamp(obj),
            Delta = obj.Delta,
            Filtered = obj.Filtered,
            Unfiltered = obj.Unfiltered,
            Rssi = obj.Rssi,
            UtcOffset = obj.UtcOffset,
            Noise = obj.Noise,
            SysTime = GetSysTime(obj),
            DirectionType = DirectionToDirectionType(obj.Direction),
        });

        _context.SaveChanges();

        return true;
    }

    private int GetDeviceId(BloodGlucoseEntry obj)
    {
        if (!_context.BglDevices.Any(d => d.Name == obj.Device))
        {
            _context.BglDevices.Add(new BglDevice
            {
                Id = default,
                Name = obj.Device,
            });
            _context.SaveChanges();
        }

        return _context.BglDevices
            .Where(d => d.Name == obj.Device)
            .First()
            .Id;
    }

    private static long GetTimestamp(BloodGlucoseEntry obj)
    {
        if (obj.Mills.HasValue && obj.Date.HasValue)
        {
            if (obj.Mills.Value != obj.Date.Value)
                throw new Exception("`mills` and `date` values are not equal");
            return obj.Mills.Value;
        }
        else if (obj.Mills.HasValue)
            return obj.Mills.Value;
        else if (obj.Date.HasValue)
            return obj.Date.Value;
        else
            throw new Exception("No `date` or `mills` property found");
    }

    private static DateTime GetSysTime(BloodGlucoseEntry obj)
    {
        if (obj.SysTime is not null && obj.DateString is not null)
        {
            var sysTime = DateTimeOffset.Parse(obj.SysTime);
            var date = DateTimeOffset.Parse(obj.DateString);
            if (sysTime != date)
                throw new Exception("`sysTime` and `dateString` values are not equal");
            return sysTime.UtcDateTime;
        }
        else if (obj.SysTime is not null)
            return DateTimeOffset.Parse(obj.SysTime).UtcDateTime;
        else if (obj.DateString is not null)
            return DateTimeOffset.Parse(obj.DateString).UtcDateTime;
        else
            throw new Exception("No `sysTime` or `dateString` property found");
    }

    [LoggerMessage(LogLevel.Information, "Imported 1 new BGL entry.")]
    protected override partial void LogOneNewEntry();

    [LoggerMessage(LogLevel.Information, "Imported {Count:N0} new BGL entries.")]
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
}
