// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Data.Paths;
using BloodSugarWatchdog.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.SingleLine = false;
});

builder.Services.AddDbContext<BloodSugarContext>(static (sp, options) =>
{
    var username = sp.GetRequiredService<IOptions<PlotOptions>>().Value.Username;
    var connectionString = ApplicationPaths.GetSqliteConnectionString(username);
    options.UseSqlite(connectionString);
});

builder.Services.AddReportService();

using var host = builder.Build();
var plotter = host.Services.GetRequiredService<IDayPlotter>();
var path = Path.Join(ApplicationPaths.GetAppCacheDirPath(), "plot.png");

plotter.RenderToPath(path);

return 0;
