// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddSystemdConsole();
builder.Services.AddBloodSugarContext();
builder.Services.AddPlottingServices();
builder.Services.AddNotifyService();

using var host = builder.Build();
var plotter = host.Services.GetRequiredService<IDayPlotter>();
var notifier = host.Services.GetRequiredService<INotifyService>();

var path = Path.GetTempFileName();
plotter.RenderToPath(path);
var success = await notifier.PostImageAsync(path, caption: args[0]);

if (File.Exists(path))
    File.Delete(path);

if (success)
    return 0;
else
    return 1;
