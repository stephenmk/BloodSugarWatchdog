using BloodSugarWatchdog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Import;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportServices(this IServiceCollection services, string username)
    {
        return services
            .AddDbContext<Context>(options =>
            {
                options.UseSqlite(ApplicationPaths.GetSqliteConnectionString(username));
                // Disable EntityFramework logging
                options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddFilter(_ => false); }));
            })
            .AddTransient<BglImporter>()
            .AddTransient<TreatmentImporter>()
            .AddLogging();
    }
}
