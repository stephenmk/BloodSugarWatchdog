// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using BloodSugarBot.Data;
using BloodSugarBot.Monitor;
using BloodSugarBot.Monitor.Checkers;
using BloodSugarBot.Nightscout;
using BloodSugarBot.Notify;
using BloodSugarBot.Plotting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Logging
    .AddSystemdConsole();

builder.Services
    .AddBloodSugarContext()
    .AddNightscoutService()
    .AddPlottingServices()
    .AddNotifyService();

builder.Services
    .AddOptions<MonitorOptions>()
    .BindConfiguration(MonitorOptions.ConfigSectionPath)
    .ValidateOnStart();

builder.Services
    .AddTransient<LowChecker>()
    .AddTransient<CrashChecker>()
    .AddTransient<SpikeChecker>()
    .AddHostedService<MonitorService>();

using var host = builder.Build();

await host.RunAsync();
