// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import.Importers;

public sealed class TreatmentImporter : Importer
{
    public TreatmentImporter(ILogger<TreatmentImporter> logger, BloodSugarContext context) : base(logger, context) { }

    protected override void Initialize()
    {
        _context.TreatmentDevices.Load();
    }

    protected override bool AddObject(JsonObject obj)
    {
        var id = (string)obj["_id"]!;

        if (_context.Treatments.Any(treatment => treatment.Id == id))
            return false;

        _context.Treatments.Add(new Treatment
        {
            Id = id,
            EventType = (string)obj["eventType"]!,
            DeviceId = GetDeviceId(_context, obj),
            Timestamp = GetTimestamp(obj),
            UUID = (string?)obj["uuid"],
            Insulin = (double?)obj["insulin"] is double insulin
                ? Math.Round(insulin, 3)
                : null,
            InsulinType = (string?)obj["insulinType"],
            InsulinInjections = (string?)obj["insulinInjections"],
            Carbs = (double?)obj["carbs"],
            Notes = (string?)obj["notes"],
            UtcOffset = (int)obj["utcOffset"]!,
            SysTime = GetSysTime(obj),
        });

        return true;
    }

    private static int GetDeviceId(BloodSugarContext context, JsonObject obj)
    {
        var name = (string?)obj["enteredBy"];

        if (!context.TreatmentDevices.Any(d => d.Name == name))
        {
            context.TreatmentDevices.Add(new TreatmentDevice
            {
                Id = default,
                Name = name,
            });
            context.SaveChanges();
        }

        return context.TreatmentDevices
            .Where(d => d.Name == name)
            .First()
            .Id;
    }

    private static long? GetTimestamp(JsonObject obj)
    {
        if (obj.ContainsKey("mills") && obj.ContainsKey("timestamp"))
        {
            var mills = (long)(double)obj["mills"]!;
            var timestamp = (long)(double)obj["timestamp"]!;
            if (mills != timestamp)
                throw new Exception("`mills` and `timestamp` values are not equal");
            return mills;
        }
        else if (obj.TryGetPropertyValue("mills", out var m))
            return (long)(double)m!;
        else if (obj.TryGetPropertyValue("timestamp", out var t))
            return (long)(double)t!;
        else if (obj.TryGetPropertyValue("date", out var d))
            return (long)(double)d!;
        else
            return null;
    }

    private static DateTime GetSysTime(JsonObject obj)
    {
        if (obj.ContainsKey("sysTime") && obj.ContainsKey("created_at"))
        {
            var sysTime = DateTime.Parse((string)obj["sysTime"]!).ToUniversalTime();
            var createdAt = DateTime.Parse((string)obj["created_at"]!).ToUniversalTime();
            if (sysTime != createdAt)
                throw new Exception("`sysTime` and `created_at` values are not equal");
            return sysTime;
        }
        else if (obj.ContainsKey("sysTime"))
            return DateTime.Parse((string)obj["sysTime"]!).ToUniversalTime();
        else if (obj.ContainsKey("created_at"))
            return DateTime.Parse((string)obj["created_at"]!).ToUniversalTime();
        else
            throw new Exception("No `sysTime` or `created_at` property found");
    }

    protected override FrozenSet<string> KnownProperties { get; } = new HashSet<string>()
    {
        "_id",
        "carbs",
        "created_at",
        "date",
        "enteredBy",
        "eventType",
        "insulin",
        "insulinInjections",
        "insulinType",
        "mills",
        "notes",
        "sysTime",
        "timestamp",
        "utcOffset",
        "uuid",
    }
    .ToFrozenSet();
}
