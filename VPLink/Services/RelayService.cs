using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VpSharp.Extensions;

namespace VPLink.Services;

internal sealed class RelayService : BackgroundService
{
    private readonly ILogger<RelayService> _logger;
    private readonly IAvatarService _avatarService;
    private readonly IDiscordMessageService _discordService;
    private readonly IVirtualParadiseMessageService _virtualParadiseService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="discordService">The Discord service.</param>
    /// <param name="avatarService">The avatar service.</param>
    /// <param name="virtualParadiseService">The Virtual Paradise service.</param>
    public RelayService(ILogger<RelayService> logger,
        IAvatarService avatarService,
        IDiscordMessageService discordService,
        IVirtualParadiseMessageService virtualParadiseService)
    {
        _logger = logger;
        _discordService = discordService;
        _avatarService = avatarService;
        _virtualParadiseService = virtualParadiseService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");

        _avatarService.OnAvatarJoined.SubscribeAsync(_discordService.AnnounceArrival);
        _avatarService.OnAvatarLeft.SubscribeAsync(_discordService.AnnounceDeparture);
        _discordService.OnMessageReceived.SubscribeAsync(_virtualParadiseService.SendMessageAsync);
        _virtualParadiseService.OnMessageReceived.SubscribeAsync(_discordService.SendMessageAsync);

        return Task.CompletedTask;
    }
}
