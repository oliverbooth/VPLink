using System.Reactive.Linq;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VpSharp;
using VpSharp.Extensions;

namespace VpBridge.Services;

internal sealed class RelayService : BackgroundService
{
    private readonly ILogger<RelayService> _logger;
    private readonly IDiscordService _discordService;
    private readonly IVirtualParadiseService _virtualParadiseService;
    private readonly DiscordSocketClient _discordClient;
    private readonly VirtualParadiseClient _virtualParadiseClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="discordService">The Discord service.</param>
    /// <param name="virtualParadiseService">The Virtual Paradise service.</param>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public RelayService(ILogger<RelayService> logger,
        IDiscordService discordService,
        IVirtualParadiseService virtualParadiseService,
        DiscordSocketClient discordClient,
        VirtualParadiseClient virtualParadiseClient)
    {
        _logger = logger;
        _discordService = discordService;
        _virtualParadiseService = virtualParadiseService;
        _discordClient = discordClient;
        _virtualParadiseClient = virtualParadiseClient;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");

        _discordService.OnMessageReceived
            .Where(m => m.Author != _discordClient.CurrentUser)
            .SubscribeAsync(_virtualParadiseService.SendMessageAsync);

        _virtualParadiseService.OnMessageReceived
            .Where(m => m.Author != _virtualParadiseClient.CurrentAvatar)
            .SubscribeAsync(_discordService.SendMessageAsync);

        return Task.CompletedTask;
    }
}
