using System.Reflection;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VpSharp;

namespace VPLink.Services;

internal sealed class BotService : BackgroundService
{
    private readonly ILogger<BotService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BotService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public BotService(ILogger<BotService> logger)
    {
        _logger = logger;
        var attribute = typeof(BotService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Version = attribute?.InformationalVersion ?? "Unknown";

        attribute = typeof(DiscordSocketClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        DiscordNetVersion = attribute?.InformationalVersion ?? "Unknown";

        attribute = typeof(VirtualParadiseClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        VpSharpVersion = attribute?.InformationalVersion ?? "Unknown";
    }

    /// <summary>
    ///     Gets the Discord.Net version.
    /// </summary>
    /// <value>The Discord.Net version.</value>
    public string DiscordNetVersion { get; }

    /// <summary>
    ///     Gets the date and time at which the bot was started.
    /// </summary>
    /// <value>The start timestamp.</value>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>
    ///     Gets the bot version.
    /// </summary>
    /// <value>The bot version.</value>
    public string Version { get; }

    /// <summary>
    ///     Gets the VP# version.
    /// </summary>
    /// <value>The VP# version.</value>
    public string VpSharpVersion { get; }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StartedAt = DateTimeOffset.UtcNow;
        _logger.LogInformation("VPLink v{Version} is starting...", Version);
        _logger.LogInformation("Discord.Net v{Version}", DiscordNetVersion);
        _logger.LogInformation("VP# v{Version}", VpSharpVersion);
        return Task.CompletedTask;
    }
}
