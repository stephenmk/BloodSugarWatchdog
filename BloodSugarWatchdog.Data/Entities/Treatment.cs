using System.ComponentModel.DataAnnotations.Schema;

namespace BloodSugarWatchdog.Data.Entities;

public sealed class Treatment
{
    public required string Id { get; init; }
    public required string EventType { get; init; }
    public required int TreatmentDeviceId { get; init; }
    public required string UUID { get; init; }
    public required double Insulin { get; init; }
    public required string InsulinInjections { get; init; }
    public required string? Carbs { get; init; }
    public required long Timestamp { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime SysTime { get; init; }
    public required int UtcOffset { get; init; }

    [ForeignKey(nameof(TreatmentDeviceId))]
    public TreatmentDevice Device { get; init; } = null!;
}
