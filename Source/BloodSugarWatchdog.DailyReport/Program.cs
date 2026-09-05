// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Logging
    .AddSystemdConsole();

builder.Services
    .AddBloodSugarContext()
    .AddPlottingServices()
    .AddNotifyService();

bool success;

using (var host = builder.Build())
{
    var plotter = host.Services.GetRequiredService<IDayPlotter>();
    var notify = host.Services.GetRequiredService<INotifyService>();

    var path = Path.GetTempFileName();
    plotter.RenderToPath(path);
    success = await notify.PostImageAsync(path, caption: args[0]);

    if (File.Exists(path))
        File.Delete(path);
}

return success ? 0 : 1;
