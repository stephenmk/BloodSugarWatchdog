// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarBot.Data;
using BloodSugarBot.Data.Entities;
using BloodSugarBot.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodSugarBot.Import.Importers;

internal sealed partial class TreatmentImporter
(
    ILogger<TreatmentImporter> logger,
    BloodSugarContext context
) :
    Importer<BloodGlucoseTreatment>(logger, context)
{
    protected override void Initialize()
    {
        _context.TreatmentDevices.Load();
    }

    protected override bool AddObject(BloodGlucoseTreatment obj)
    {
        if (_context.Treatments.Any(t => t.Id == obj.Id))
            return false;

        _context.Treatments.Add(new()
        {
            Id = obj.Id,
            EventType = obj.EventType,
            DeviceId = GetDeviceId(obj),
            Timestamp = GetTimestamp(obj),
            UUID = obj.Uuid,
            Insulin = obj.Insulin.HasValue
                ? Math.Round(obj.Insulin.Value, 3)
                : null,
            InsulinType = obj.InsulinType,
            InsulinInjections = obj.InsulinInjections,
            Carbs = obj.Carbs,
            Notes = obj.Notes,
            UtcOffset = obj.UtcOffset,
            SysTime = GetSysTime(obj),
        });

        _context.SaveChanges();

        return true;
    }

    private int GetDeviceId(BloodGlucoseTreatment obj)
    {
        if (!_context.TreatmentDevices.Any(d => d.Name == obj.EnteredBy))
        {
            _context.TreatmentDevices.Add(new TreatmentDevice
            {
                Id = default,
                Name = obj.EnteredBy,
            });
            _context.SaveChanges();
        }

        return _context.TreatmentDevices
            .Where(d => d.Name == obj.EnteredBy)
            .First()
            .Id;
    }

    private static long? GetTimestamp(BloodGlucoseTreatment obj)
    {
        if (obj.Mills.HasValue && obj.Timestamp.HasValue)
        {
            if (obj.Mills.Value != obj.Timestamp.Value)
                throw new Exception("`mills` and `timestamp` values are not equal");
            return obj.Mills.Value;
        }
        else if (obj.Mills.HasValue)
            return obj.Mills.Value;
        else if (obj.Timestamp.HasValue)
            return obj.Timestamp.Value;
        else if (obj.Date.HasValue)
            return obj.Date.Value;
        else
            return null;
    }

    private static DateTime GetSysTime(BloodGlucoseTreatment obj)
    {
        if (obj.SysTime is not null && obj.CreatedAt is not null)
        {
            var sysTime = DateTimeOffset.Parse(obj.SysTime);
            var date = DateTimeOffset.Parse(obj.CreatedAt);
            if (sysTime != date)
                throw new Exception("`sysTime` and `created_at` values are not equal");
            return sysTime.UtcDateTime;
        }
        else if (obj.SysTime is not null)
            return DateTimeOffset.Parse(obj.SysTime).UtcDateTime;
        else if (obj.CreatedAt is not null)
            return DateTimeOffset.Parse(obj.CreatedAt).UtcDateTime;
        else
            throw new Exception("No `sysTime` or `created_at` property found");
    }

    [LoggerMessage(LogLevel.Information, "Imported 1 new treatment entry.")]
    protected override partial void LogOneNewEntry();

    [LoggerMessage(LogLevel.Information, "Imported {Count:N0} new treatment entries.")]
    protected override partial void LogMultipleNewEntries(int count);
}
