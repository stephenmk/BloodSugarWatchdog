using System.ComponentModel.DataAnnotations.Schema;
using BloodSugarWatchdog.Data.Enums;

namespace BloodSugarWatchdog.Data.Entities;

public sealed class Bgl
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required int DeviceId { get; init; }
    public required int Sgv { get; init; }
    public required string Delta { get; init; }
    public required DirectionType DirectionType { get; init; }
    public required int Filtered { get; init; }
    public required int Unfiltered { get; init; }
    public required int Rssi { get; init; }
    public required int Noise { get; init; }
    public required long Timestamp { get; init; }
    public required DateTime SysTime { get; init; }
    public required int UtcOffset { get; init; }

    [ForeignKey(nameof(DirectionType))]
    public Direction Direction { get; init; } = null!;

    [ForeignKey(nameof(DeviceId))]
    public Device Device { get; init; } = null!;
}
