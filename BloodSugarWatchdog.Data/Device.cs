namespace BloodSugarWatchdog.Data;

public sealed class Device
{
    public required int Id { get; init; }
    public required string Name { get; init; }

    public List<Bgl> Bgls { get; init; } = [];
}
