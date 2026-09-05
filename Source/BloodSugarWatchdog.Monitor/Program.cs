// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Monitor;
using BloodSugarWatchdog.Monitor.Checkers;
using BloodSugarWatchdog.Nightscout;
using BloodSugarWatchdog.Notify;
using BloodSugarWatchdog.Plotting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Logging
    .AddSystemdConsole();

builder.Services
    .AddOptions<MonitorOptions>()
    .BindConfiguration(MonitorOptions.ConfigSectionPath)
    .ValidateOnStart();

builder.Services
    .AddBloodSugarContext()
    .AddNightscoutService()
    .AddPlottingServices()
    .AddNotifyService();

builder.Services
    .AddTransient<LowChecker>()
    .AddTransient<CrashChecker>()
    .AddHostedService<MonitorService>();

using var host = builder.Build();

await host.RunAsync();
