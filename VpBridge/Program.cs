﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Tomlyn.Extensions.Configuration;
using VpBridge.Services;
using VpSharp;
using X10D.Hosting.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddSingleton<VirtualParadiseClient>();
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
}));

builder.Services.AddHostedSingleton<IVirtualParadiseService, VirtualParadiseService>();
builder.Services.AddHostedSingleton<IDiscordService, DiscordService>();
builder.Services.AddHostedSingleton<RelayService>();

await builder.Build().RunAsync();