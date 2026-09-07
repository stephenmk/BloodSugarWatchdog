// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Paths;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Logging.AddSimpleConsole(static options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.SingleLine = false;
});

builder.Services.AddBloodSugarContext();
builder.Services.AddPlottingServices();

using var host = builder.Build();
var plotter = host.Services.GetRequiredService<IDayPlotter>();
var path = Path.Join(ApplicationPaths.GetAppCacheDirPath(), "plot.png");

plotter.RenderToPath(path);

return 0;
