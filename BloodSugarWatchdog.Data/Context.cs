using BloodSugarWatchdog.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BloodSugarWatchdog.Data;

public sealed class Context : DbContext
{
    private readonly string _dbPath;

    public Context(string username)
    {
        var dirPath = Path.Join(EnvironmentPaths.LocalDataPath, "BloodSugarWatchdog", username);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Join(dirPath, "data.db"),
            Pooling = true,
        };
        _dbPath = builder.ToString();
    }

    protected sealed override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(_dbPath);

    public DbSet<Bgl> Bgls { get; set; } = null!;
    public DbSet<Direction> Directions { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
}
