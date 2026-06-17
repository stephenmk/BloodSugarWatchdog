using System.Text.Json.Nodes;
using BloodSugarWatchdog.Data;
using Microsoft.EntityFrameworkCore;

namespace BloodSugarWatchdog.Import;

internal sealed class BglImporter : Importer
{
    protected override void Initialize(Context context)
    {
        context.Devices.Load();
        InitializeDirections(context);
    }

    private static void InitializeDirections(Context context)
    {
        context.Directions.Load();
        if (context.Directions.Any())
        {
            return;
        }
        foreach (var type in Enum.GetValues<DirectionType>())
        {
            context.Directions.Add(new Direction
            {
                Type = type,
                Name = type.ToString(),
            });
        }
    }

    protected override void ProcessObj(Context context, JsonObject obj)
    {
        var id = (string)obj["_id"]!;
        if (context.Bgls.Any(b => b.Id == id))
        {
            return;
        }

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
    }

    private static int GetDeviceId(Context context, JsonObject obj)
    {
        var name = (string)obj["device"]!;
        if (!context.Devices.Any(d => d.Name == name))
        {
            context.Devices.Add(new Device
            {
                Id = default,
                Name = name,
            });
            context.SaveChanges();
        }
        return context.Devices.Where(d => d.Name == name).First().Id;
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
    private static DirectionType DirectionToDirectionType(string direction) => direction switch
    {
        "NONE"              => DirectionType.None,
        "DoubleUp"          => DirectionType.DoubleUp,
        "SingleUp"          => DirectionType.SingleUp,
        "FortyFiveUp"       => DirectionType.FortyFiveUp,
        "Flat"              => DirectionType.Flat,
        "FortyFiveDown"     => DirectionType.FortyFiveDown,
        "SingleDown"        => DirectionType.SingleDown,
        "DoubleDown"        => DirectionType.DoubleDown,
        "NOT COMPUTABLE"    => DirectionType.NotComputable,
        "RATE OUT OF RANGE" => DirectionType.RateOutOfRange,
        _                   => throw new ArgumentOutOfRangeException(nameof(direction))
    };
    #pragma warning restore format
}
