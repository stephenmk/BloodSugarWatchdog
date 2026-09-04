// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodSugarWatchdog.Notify;

/// <summary>
/// Command-line executable for testing purposes.
/// </summary>
internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.SingleLine = false;
        });

        builder.Services.AddNotifyService();

        using var host = builder.Build();

        var notifier = host.Services.GetRequiredService<INotifyService>();

        await notifier.PostImageAsync("/home/stephen/.cache/BloodSugarWatchdog/plot.png", "test", default);
    }
}
