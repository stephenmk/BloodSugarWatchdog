using System.Collections.Frozen;
using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Entities;
using BloodSugarWatchdog.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodSugarWatchdog.Import;

public sealed class BglImporter : Importer
{
    protected override void Initialize(Context context)
    {
        context.BglDevices.Load();
        InitializeDirections(context);
    }

    private static void InitializeDirections(Context context)
    {
        context.BglDirections.Load();
        if (context.BglDirections.Any())
        {
            return;
        }
        foreach (var type in Enum.GetValues<BglDirectionType>())
        {
            context.BglDirections.Add(new BglDirection
            {
                Type = type,
                Name = type.ToString(),
            });
        }
    }

    protected override bool ProcessObj(Context context, JsonObject obj)
    {
        var id = (string)obj["_id"]!;

        if (context.Bgls.Any(bgl => bgl.Id == id))
            return false;

        context.Bgls.Add(new Bgl
        {
            Id = id,
            Type = (string)obj["type"]!,
            DeviceId = GetDeviceId(context, obj),
            Sgv = (int)obj["sgv"]!,
            Timestamp = GetTimestamp(obj),
            Delta = obj["delta"]!.ToString(),
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

    private static int GetDeviceId(Context context, JsonObject obj)
    {
        var name = (string)obj["device"]!;

        if (!context.BglDevices.Any(d => d.Name == name))
        {
            context.BglDevices.Add(new BglDevice
            {
                Id = default,
                Name = name,
            });
            context.SaveChanges();
        }

        return context.BglDevices
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
            var sysTime = DateTime.Parse((string)obj["sysTime"]!);
            var date = DateTime.Parse((string)obj["dateString"]!);
            if (sysTime != date)
                throw new Exception("`sysTime` and `dateString` values are not equal");
            return sysTime;
        }
        else if (obj.ContainsKey("sysTime"))
            return DateTime.Parse((string)obj["sysTime"]!);
        else if (obj.ContainsKey("dateString"))
            return DateTime.Parse((string)obj["dateString"]!);
        else
            throw new Exception("No `sysTime` or `dateString` property found");
    }

    #pragma warning disable format
    private static BglDirectionType DirectionToDirectionType(string direction) => direction switch
    {
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
    };
    #pragma warning restore format

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
