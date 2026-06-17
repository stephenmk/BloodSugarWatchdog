using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodSugarWatchdog.Data;

[Table(nameof(Direction))]
public sealed class Direction
{
    [Key]
    public required DirectionType Type { get; init; }
    public required string Name { get; init; }

    public List<Bgl> Bgls { get; init; } = [];
}
