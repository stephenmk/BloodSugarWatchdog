using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodSugarWatchdog.Data.Enums;

namespace BloodSugarWatchdog.Data.Entities;

[Table(nameof(Direction))]
public sealed class Direction
{
    [Key]
    public required DirectionType Type { get; init; }
    public required string Name { get; init; }

    public List<Bgl> Bgls { get; init; } = [];
}
