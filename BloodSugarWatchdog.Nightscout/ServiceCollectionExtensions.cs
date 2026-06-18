using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            })
            .AddTransient(_ => serviceOptions)
            .AddTransient<NightscoutHttpClient>()
            .AddTransient<BglImporter>()
            .AddTransient<TreatmentImporter>()
            .AddTransient<INightscoutService, NightscoutService>()
            .AddLogging();
    }
}
