// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data.Paths;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Report;

internal static class Program
{
    private static int Main(string[] args)
    {
        var username = args[0];

        using var provider = GetServiceProvider(username);
        var plotter = provider.GetRequiredService<IStatusPlotter>();

        var path = Path.Join(ApplicationPaths.GetAppCacheDirPath(username), "plot.png");
        plotter.RenderToPath(path);

        return 0;
    }

    private static ServiceProvider GetServiceProvider(string username)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddReportService(options =>
        {
            options.Username = username;
        });

        serviceCollection.AddLogging(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = false;
                options.TimestampFormat = "HH:mm:ss ";
            }));

        return serviceCollection.BuildServiceProvider();
    }
}
