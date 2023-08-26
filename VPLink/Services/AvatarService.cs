using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Common.Configuration;
using VPLink.Common.Services;
using VpSharp;
using VpSharp.Entities;

namespace VPLink.Services;

/// <inheritdoc cref="IAvatarService" />
internal sealed class AvatarService : BackgroundService, IAvatarService
{
    private readonly ILogger<AvatarService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly VirtualParadiseClient _virtualParadiseClient;
    private readonly Subject<VirtualParadiseAvatar> _avatarJoined = new();
    private readonly Subject<VirtualParadiseAvatar> _avatarLeft = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AvatarService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public AvatarService(ILogger<AvatarService> logger,
        IConfigurationService configurationService,
        VirtualParadiseClient virtualParadiseClient)
    {
        _logger = logger;
        _configurationService = configurationService;
        _virtualParadiseClient = virtualParadiseClient;
    }

    /// <inheritdoc />
    public IObservable<VirtualParadiseAvatar> OnAvatarJoined => _avatarJoined.AsObservable();

    /// <inheritdoc />
    public IObservable<VirtualParadiseAvatar> OnAvatarLeft => _avatarLeft.AsObservable();

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _virtualParadiseClient.AvatarJoined.Subscribe(OnVPAvatarJoined);
        _virtualParadiseClient.AvatarLeft.Subscribe(OnVPAvatarLeft);
        return Task.CompletedTask;
    }

    private void OnVPAvatarJoined(VirtualParadiseAvatar avatar)
    {
        _logger.LogInformation("{Avatar} joined ({User})", avatar, avatar.User);

        IBotConfiguration configuration = _configurationService.BotConfiguration;
        if (!configuration.AnnounceAvatarEvents || avatar.IsBot && !configuration.AnnounceBots)
            return;

        _avatarJoined.OnNext(avatar);
    }

    private void OnVPAvatarLeft(VirtualParadiseAvatar avatar)
    {
        _logger.LogInformation("{Avatar} left ({User})", avatar, avatar.User);

        IBotConfiguration configuration = _configurationService.BotConfiguration;
        if (!configuration.AnnounceAvatarEvents || avatar.IsBot && !configuration.AnnounceBots)
            return;

        _avatarLeft.OnNext(avatar);
    }
}
