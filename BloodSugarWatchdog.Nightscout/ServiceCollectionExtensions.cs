using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Nightscout;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNightscoutService(this IServiceCollection services, Action<NightscoutOptions> configure)
    {
        var serviceOptions = new NightscoutOptions();
        configure(serviceOptions);

        return services
            .AddDbContext<Context>(options =>
            {
                options.UseSqlite(ApplicationPaths.GetSqliteConnectionString(serviceOptions.Username));
                // Disable EntityFramework logging
                options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddFilter(_ => false); }));
            })
            .AddTransient(_ => serviceOptions)
            .AddTransient<NightscoutHttpClient>()
            .AddTransient<BglImporter>()
            .AddTransient<TreatmentImporter>()
            .AddTransient<INightscoutService, NightscoutService>()
            .AddLogging();
    }
}
